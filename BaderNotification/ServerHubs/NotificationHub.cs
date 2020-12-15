using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace BaderNotification.ServerHubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            var userId = GetUserId();

            JoinGroup(userId);

            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = GetUserId();

            LeaveGroup(userId);

            return base.OnDisconnectedAsync(exception);
        }
        private Task JoinGroup(string userId) => Groups.AddToGroupAsync(Context.ConnectionId, userId);

        private Task LeaveGroup(string userId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

        private string GetUserId() => Context.UserIdentifier;
    }
}
