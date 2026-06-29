using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace HorseRacing.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Log client connection
        var userId = Context.UserIdentifier;
        Console.WriteLine($"[SignalR] User {userId} connected on ConnectionId: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        Console.WriteLine($"[SignalR] User {userId} disconnected. ConnectionId: {Context.ConnectionId}. Exception: {exception?.Message}");
        await base.OnDisconnectedAsync(exception);
    }
}
