using System.Threading.Tasks;

namespace HorseRacing.Application.Common.Interfaces;

public interface IGoogleTokenVerifier
{
    Task<GoogleUserData?> VerifyTokenAsync(string idToken);
}

public class GoogleUserData
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GoogleId { get; set; } = string.Empty;
}
