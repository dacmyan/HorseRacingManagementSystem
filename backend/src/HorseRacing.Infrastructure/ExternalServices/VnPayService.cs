using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using HorseRacing.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HorseRacing.Infrastructure.ExternalServices;

public class VnPayService : IVnPayService
{
    private readonly IConfiguration _configuration;

    public VnPayService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreatePaymentUrl(string transactionReference, decimal amount, string orderInfo, string ipAddress)
    {
        var vnpayConfig = _configuration.GetSection("VNPay");
        var tmnCode = vnpayConfig["TmnCode"] ?? _configuration["VNPAY_TMN_CODE"] ?? string.Empty;
        var hashSecret = vnpayConfig["HashSecret"] ?? _configuration["VNPAY_HASH_SECRET"] ?? string.Empty;
        var paymentUrl = vnpayConfig["PaymentUrl"] ?? _configuration["VNPAY_PAYMENT_URL"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        var returnUrl = vnpayConfig["ReturnUrl"] ?? _configuration["VNPAY_RETURN_URL"] ?? string.Empty;

        if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret))
        {
            throw new InvalidOperationException("Cấu hình VNPay TmnCode hoặc HashSecret chưa được thiết lập.");
        }

        // VNPay expects amount as VND multiplied by 100
        long vnpAmount = (long)(amount * 100);

        // Get Vietnam time (GMT+7)
        TimeZoneInfo vTimezone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        DateTime vLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vTimezone);
        string vnpCreateDate = vLocalTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", tmnCode },
            { "vnp_Amount", vnpAmount.ToString() },
            { "vnp_CreateDate", vnpCreateDate },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", string.IsNullOrEmpty(ipAddress) ? "127.0.0.1" : ipAddress },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", orderInfo },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", returnUrl },
            { "vnp_TxnRef", transactionReference }
        };

        var rawData = string.Join("&", parameters.Select(kv => $"{kv.Key}={VnPayUrlEncode(kv.Value)}"));
        var secureHash = HmacSha512(hashSecret, rawData);
        
        var paymentUrlBuilder = new StringBuilder(paymentUrl);
        paymentUrlBuilder.Append("?");
        paymentUrlBuilder.Append(rawData);
        paymentUrlBuilder.Append("&vnp_SecureHash=");
        paymentUrlBuilder.Append(secureHash);

        return paymentUrlBuilder.ToString();
    }

    public bool ValidateCallback(IDictionary<string, string> queryParameters)
    {
        var vnpayConfig = _configuration.GetSection("VNPay");
        var hashSecret = vnpayConfig["HashSecret"] ?? _configuration["VNPAY_HASH_SECRET"] ?? string.Empty;

        if (string.IsNullOrEmpty(hashSecret))
        {
            throw new InvalidOperationException("Cấu hình VNPay HashSecret chưa được thiết lập.");
        }

        if (!queryParameters.TryGetValue("vnp_SecureHash", out var secureHash) || string.IsNullOrEmpty(secureHash))
        {
            return false;
        }

        // Sort parameters and exclude vnp_SecureHash and vnp_SecureHashType
        var sortedParams = new SortedDictionary<string, string>(queryParameters, StringComparer.Ordinal);
        sortedParams.Remove("vnp_SecureHash");
        sortedParams.Remove("vnp_SecureHashType");

        var checkRawData = string.Join("&", sortedParams.Select(kv => $"{kv.Key}={VnPayUrlEncode(kv.Value)}"));
        var calculatedHash = HmacSha512(hashSecret, checkRawData);

        return string.Equals(calculatedHash, secureHash, StringComparison.OrdinalIgnoreCase);
    }

    private string VnPayUrlEncode(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var sb = new StringBuilder();
        var bytes = Encoding.UTF8.GetBytes(value);
        foreach (var b in bytes)
        {
            if ((b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9') ||
                b == '-' || b == '_' || b == '.' || b == '*')
            {
                sb.Append((char)b);
            }
            else if (b == ' ')
            {
                sb.Append('+');
            }
            else
            {
                sb.Append('%' + b.ToString("X2"));
            }
        }
        return sb.ToString();
    }

    private string HmacSha512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower(); // hex string in lowercase
    }
}
