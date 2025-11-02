using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GreenTechMVC.Hubs
{
    /// <summary>
    /// Generic SignalR Hub for real-time notifications
    /// Can be used for avatar updates, notifications, live updates, etc.
    /// </summary>
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Join a user-specific group for personalized notifications
        /// </summary>
        public async Task JoinUserGroup(int userId)
        {
            var groupName = $"user-{userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation(
                "Client {ConnectionId} joined group {GroupName}",
                Context.ConnectionId,
                groupName
            );
        }

        /// <summary>
        /// Join a custom group for notifications
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Leave a group
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Join an order-specific group for real-time order updates
        /// </summary>
        public async Task JoinOrderGroup(int orderId)
        {
            var groupName = $"order-{orderId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation(
                "Client {ConnectionId} joined group {GroupName}",
                Context.ConnectionId,
                groupName
            );
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
