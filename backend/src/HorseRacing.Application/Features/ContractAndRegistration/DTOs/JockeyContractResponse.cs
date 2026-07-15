using System;

namespace HorseRacing.Application.Features.ContractAndRegistration.DTOs;

public class JockeyContractResponse
{
    public int Id { get; set; }
    public int HorseId { get; set; }
    public string HorseName { get; set; } = string.Empty;
    public long TournamentId { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int JockeyId { get; set; }
    public string JockeyName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal RentalFee { get; set; }
    public decimal WinningBonusPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime InvitationExpiredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
