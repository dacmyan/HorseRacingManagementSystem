using System;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class JockeyContract
{
    public int ContractId { get; set; }

    public long TournamentId { get; set; }
    public Tournament? Tournament { get; set; }

    public long HorseId { get; set; }
    public Horse? Horse { get; set; }

    public int JockeyId { get; set; }
    public AppUser? Jockey { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Pending";

    public DateTime InvitationExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}