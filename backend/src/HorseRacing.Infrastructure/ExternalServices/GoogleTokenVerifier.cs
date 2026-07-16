using System;
using System.Threading.Tasks;
using Google.Apis.Auth;
using HorseRacing.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HorseRacing.Infrastructure.ExternalServices;

public class GoogleTokenVerifier : IGoogleTokenVerifier
{
    private readonly IConfiguration _configuration;

    public GoogleTokenVerifier(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<GoogleUserData?> VerifyTokenAsync(string idToken)
    {
        try
        {
            var googleSettings = _configuration.GetSection("Google");
            var clientId = googleSettings["ClientId"];
            
            // Allow fallback to environment variable directly
            if (string.IsNullOrEmpty(clientId) || clientId == "GOOGLE_CLIENT_ID")
            {
                clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
            }

            var validationSettings = new GoogleJsonWebSignature.ValidationSettings();
            if (!string.IsNullOrEmpty(clientId))
            {
                validationSettings.Audience = new[] { clientId };
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
            if (payload == null)
            {
                return null;
            }

            return new GoogleUserData
            {
                Email = payload.Email,
                Name = payload.Name ?? payload.GivenName ?? payload.FamilyName ?? string.Empty,
                GoogleId = payload.Subject
            };
        }
        catch (Exception)
        {
            // Invalid, expired, or malformed token
            return null;
        }
    }
}
