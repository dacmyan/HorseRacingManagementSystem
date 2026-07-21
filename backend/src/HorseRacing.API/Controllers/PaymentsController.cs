using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacing.Application.Common.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HorseRacing.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPayOSService _payOSService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        AppDbContext context,
        IPayOSService payOSService,
        IConfiguration configuration,
        ILogger<PaymentsController> logger)
    {
        _context = context;
        _payOSService = payOSService;
        _configuration = configuration;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nameIdentifier))
        {
            nameIdentifier = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        }
        return int.Parse(nameIdentifier ?? "0");
    }

    [HttpPost("payos/create-deposit")]
    [Authorize]
    public async Task<IActionResult> CreateDeposit([FromBody] CreateDepositRequest request)
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        // Validate Amount >= 10,000 VND
        if (request.Amount < 10000)
        {
            return BadRequest(new { message = "Số tiền nạp tối thiểu là 10,000 VND." });
        }

        // Retrieve or create User's Wallet
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
        {
            wallet = new Wallet { UserId = userId, Balance = 0 };
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
        }

        // Create PENDING WalletTransaction
        var transaction = new WalletTransaction
        {
            WalletId = wallet.WalletId,
            Amount = request.Amount,
            Type = "Deposit",
            Status = "PENDING",
            PaymentMethod = "PayOS",
            Description = string.IsNullOrEmpty(request.OrderInfo) ? "Nap tien vao vi tai khoan" : request.OrderInfo,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Generate unique 53-bit transaction reference using the inserted ID
        long orderCode = long.Parse(DateTimeOffset.UtcNow.ToString("yyMMddHHmmss") + (transaction.TransactionId % 100).ToString("00"));
        transaction.GatewayTransactionId = orderCode.ToString();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deposit transaction created: User {UserId}, Amount {Amount}, Ref {TxnRef}", userId, request.Amount, orderCode);

        // Construct dynamic backend return URL based on config (or fallback to current request)
        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:5173";
        if (Request.Host.Host.Contains("azurewebsites.net"))
        {
            frontendUrl = "https://horse-tournament-management.vercel.app";
        }

        string returnUrl = $"{frontendUrl}/payment/payos/return";
        string cancelUrl = $"{frontendUrl}/payment/payos/cancel";

        string paymentUrl = await _payOSService.CreatePaymentLinkAsync(orderCode, (int)request.Amount, transaction.Description, returnUrl, cancelUrl);

        return Ok(new
        {
            paymentUrl,
            transactionReference = orderCode.ToString()
        });
    }

    [HttpPost("payos-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOSWebhook([FromBody] PayOS.Models.Webhooks.Webhook webhookBody)
    {
        _logger.LogInformation("PayOS webhook received");

        try
        {
            // Verify Signature
            var webhookData = await _payOSService.VerifyWebhookAsync(webhookBody);

            var orderCode = webhookData.OrderCode.ToString();

            // PayOS webhooks are only sent on SUCCESS
            await ProcessPaymentConfirmationAsync(orderCode, webhookData.Amount, "SUCCESS");

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in PayOS Webhook Handler");
            return Ok(new { success = false, message = "Error processing webhook" });
        }
    }

    private async Task ProcessPaymentConfirmationAsync(
        string txnRef, decimal amount, string status)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.GatewayTransactionId == txnRef);

        if (transaction == null)
        {
            _logger.LogWarning("Transaction not found for Ref {TxnRef}", txnRef);
            return;
        }

        if (transaction.Amount != amount)
        {
            _logger.LogWarning("Amount mismatch for Ref {TxnRef}. DB: {DbAmount}, PayOS: {PayOsAmount}", txnRef, transaction.Amount, amount);
            return;
        }

        if (transaction.Status != "PENDING")
        {
            _logger.LogInformation("Duplicate transaction confirmation ignored for Ref {TxnRef}. Current Status: {Status}", txnRef, transaction.Status);
            return;
        }

        using var dbTxn = await _context.Database.BeginTransactionAsync();
        try
        {
            if (status == "SUCCESS")
            {
                transaction.Status = "SUCCESS";
                transaction.Description = $"{transaction.Description} | Paid: {DateTime.UtcNow.AddHours(7):dd/MM/yyyy HH:mm:ss}";

                var wallet = await _context.Wallets.FindAsync(transaction.WalletId);
                if (wallet != null)
                {
                    decimal coinsEarned = transaction.Amount / 250m;
                    wallet.Balance += coinsEarned;
                    _context.Wallets.Update(wallet);
                }

                _logger.LogInformation("Wallet credited successfully: User {UserId}, Amount {Amount} VND ({Coins} coins), Ref {TxnRef}", wallet?.UserId, transaction.Amount, transaction.Amount / 250m, txnRef);
            }
            else
            {
                transaction.Status = "FAILED";
                transaction.Description = $"{transaction.Description} | Payer Cancelled/Failed";
                _logger.LogWarning("Payment failed for Ref {TxnRef}", txnRef);
            }

            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
            await dbTxn.CommitAsync();
        }
        catch (Exception ex)
        {
            await dbTxn.RollbackAsync();
            _logger.LogError(ex, "Error occurred during wallet deposit confirmation for Ref {TxnRef}", txnRef);
            throw;
        }
    }

    [HttpGet("payos/{transactionReference}/status")]
    [Authorize]
    public async Task<IActionResult> GetPaymentStatus(string transactionReference)
    {
        var userId = GetCurrentUserId();
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "None";

        var transaction = await _context.Transactions
            .Include(t => t.Wallet)
            .FirstOrDefaultAsync(t => t.GatewayTransactionId == transactionReference);

        if (transaction == null)
        {
            return NotFound(new { message = "Transaction not found." });
        }

        if (transaction.Wallet?.UserId != userId && userRole != "Admin")
        {
            return Forbid();
        }

        return Ok(new
        {
            transactionId = transaction.TransactionId,
            transactionReference = transaction.GatewayTransactionId,
            amount = transaction.Amount,
            status = transaction.Status,
            type = transaction.Type,
            description = transaction.Description,
            createdAt = transaction.CreatedAt
        });
    }
}

public class CreateDepositRequest
{
    public decimal Amount { get; set; }
    public string OrderInfo { get; set; } = "Nap tien vao vi tai khoan";
}
