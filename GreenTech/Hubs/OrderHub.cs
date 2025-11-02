using Microsoft.AspNetCore.SignalR;

namespace GreenTech.Hubs
{
    /// <summary>
    /// SignalR Hub for order-related real-time notifications
    /// Shared between Razor Pages (Admin) and MVC (Customer)
    /// </summary>
    public class OrderHub : Hub
    {
        /// <summary>
        /// Join a user-specific group for personalized order notifications
        /// </summary>
        public async Task JoinUserGroup(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        /// <summary>
        /// Join an order-specific group for real-time order updates
        /// </summary>
        public async Task JoinOrderGroup(int orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }

        /// <summary>
        /// Leave an order group
        /// </summary>
        public async Task LeaveOrderGroup(int orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }
    }
}
