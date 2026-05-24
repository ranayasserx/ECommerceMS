using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs
{
    public class NotificationHub : Hub
    {
        // Called when a client connects
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", $"Connected with ID: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        // Allows clients to join a group by customerId
        // so notifications are sent only to the right user
        public async Task JoinCustomerGroup(string customerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, customerId);
            await Clients.Caller.SendAsync("JoinedGroup", $"Joined group: {customerId}");
        }

        public async Task LeaveCustomerGroup(string customerId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, customerId);
        }
    }
}