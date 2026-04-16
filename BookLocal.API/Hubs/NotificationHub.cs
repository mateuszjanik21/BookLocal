using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class NotificationHub : Hub
{
    private readonly AppDbContext _context;

    public NotificationHub(AppDbContext context)
    {
        _context = context;
    }

    public async Task JoinBusinessGroup(string businessId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null && int.TryParse(businessId, out int bId))
        {
            var isOwner = await _context.Businesses.AnyAsync(b => b.BusinessId == bId && b.OwnerId == userId);
            if (isOwner)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, businessId);
            }
        }
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"--> Client connected to NotificationHub: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }
}