namespace HorseRacing.Domain.Entities;

public class Wallet
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public decimal Balance { get; set; }
}
