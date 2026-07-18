using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacing.Infrastructure.Persistence;

namespace HorseRacing.API.Filters;

public class BlockLockedUserAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var user = httpContext.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var nameIdentifier = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nameIdentifier))
            {
                nameIdentifier = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            }

            if (int.TryParse(nameIdentifier, out var userId))
            {
                var dbContext = httpContext.RequestServices.GetRequiredService<AppDbContext>();
                var appUser = await dbContext.Users.FindAsync(userId);
                if (appUser != null && !string.Equals(appUser.Status, "Active", System.StringComparison.OrdinalIgnoreCase))
                {
                    context.Result = new BadRequestObjectResult(new
                    {
                        message = "Your account is not active. You are not allowed to perform this operation, except for withdrawal!"
                    });
                    return;
                }
            }
        }

        await next();
    }
}
