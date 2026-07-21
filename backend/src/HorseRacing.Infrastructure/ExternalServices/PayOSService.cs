using System;
using System.Threading.Tasks;
using HorseRacing.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models.V2.PaymentRequests;

namespace HorseRacing.Infrastructure.ExternalServices;

public class PayOSService : IPayOSService
{
    private readonly PayOSClient _payOS;
    private readonly ILogger<PayOSService> _logger;

    public PayOSService(IConfiguration configuration, ILogger<PayOSService> logger)
    {
        _logger = logger;
        var clientId = configuration["PayOS:ClientId"] ?? throw new ArgumentNullException("PayOS:ClientId");
        var apiKey = configuration["PayOS:ApiKey"] ?? throw new ArgumentNullException("PayOS:ApiKey");
        var checksumKey = configuration["PayOS:ChecksumKey"] ?? throw new ArgumentNullException("PayOS:ChecksumKey");
        _payOS = new PayOSClient(clientId, apiKey, checksumKey);
    }

    public async Task<string> CreatePaymentLinkAsync(long orderCode, int amount, string description, string returnUrl, string cancelUrl)
    {
        try
        {
            var request = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = amount,
                Description = description,
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
            };

            var response = await _payOS.PaymentRequests.CreateAsync(request);
            return response.CheckoutUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayOS payment link for orderCode {OrderCode}", orderCode);
            throw;
        }
    }

    public async Task<(long OrderCode, decimal Amount)> VerifyWebhookAsync(object webhookBody)
    {
        try
        {
            if (webhookBody is PayOS.Models.Webhooks.Webhook webhook)
            {
                var data = await _payOS.Webhooks.VerifyAsync(webhook);
                return (data.OrderCode, data.Amount);
            }
            throw new ArgumentException("Invalid webhook body type.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying PayOS webhook data");
            throw;
        }
    }
}
