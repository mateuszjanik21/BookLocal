using Microsoft.AspNetCore.SignalR;
public class NotificationHub : Hub
{
    public async Task JoinBusinessGroup(string businessId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, businessId);
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"--> Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }
}