using System.Threading.Tasks;

namespace HorseRacing.Application.Common.Interfaces;

public interface IPayOSService
{
    Task<string> CreatePaymentLinkAsync(long orderCode, int amount, string description, string returnUrl, string cancelUrl);
    Task<(long OrderCode, decimal Amount)> VerifyWebhookAsync(object webhookBody);
}
