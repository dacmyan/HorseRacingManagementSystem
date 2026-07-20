using System.Collections.Generic;

namespace HorseRacing.Application.Common.Interfaces;

public interface IVnPayService
{
    string CreatePaymentUrl(string transactionReference, decimal amount, string orderInfo, string ipAddress);
    bool ValidateCallback(IDictionary<string, string> queryParameters);
}
