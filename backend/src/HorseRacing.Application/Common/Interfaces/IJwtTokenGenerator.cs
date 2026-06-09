using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(AppUser user);
}
