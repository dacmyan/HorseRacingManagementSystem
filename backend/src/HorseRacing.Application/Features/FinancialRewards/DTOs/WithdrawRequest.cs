using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.FinancialRewards.DTOs;

public class WithdrawRequest
{
    [Range(typeof(decimal), "1", "1000000000000")]
    public decimal Amount { get; set; }
}
