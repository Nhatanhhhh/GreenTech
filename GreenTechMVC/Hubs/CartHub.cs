using Microsoft.AspNetCore.SignalR;

namespace GreenTechMVC.Hubs
{
    public class CartHub : Hub
    {
        public async Task JoinCartGroup(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
    }
}


