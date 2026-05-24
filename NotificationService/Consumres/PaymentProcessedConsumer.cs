using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Events;
using NotificationService.Hubs;

namespace NotificationService.Consumers
{
    public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<PaymentProcessedConsumer> _logger;

        public PaymentProcessedConsumer(IHubContext<NotificationHub> hubContext, ILogger<PaymentProcessedConsumer> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
        {
            var evt = context.Message;
            _logger.LogInformation("Sending notification for Order {OrderId} - Payment {Status}",
                evt.OrderId, evt.IsSuccessful ? "Succeeded" : "Failed");

            var notification = new
            {
                orderId = evt.OrderId,
                isSuccessful = evt.IsSuccessful,
                amount = evt.Amount,
                message = evt.IsSuccessful
                    ? $"Payment of {evt.Amount:C} for Order #{evt.OrderId} was successful!"
                    : $"Payment for Order #{evt.OrderId} failed: {evt.FailureReason}",
                processedAt = evt.ProcessedAt
            };

            // Send to the specific customer's group
            await _hubContext.Clients.Group(evt.CustomerId)
                .SendAsync("PaymentNotification", notification);

            // Also send to all connected clients (for admin dashboard)
            await _hubContext.Clients.All
                .SendAsync("PaymentUpdate", notification);
        }
    }

    public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<OrderPlacedConsumer> _logger;

        public OrderPlacedConsumer(IHubContext<NotificationHub> hubContext, ILogger<OrderPlacedConsumer> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var evt = context.Message;
            _logger.LogInformation("Sending order confirmation notification for Order {OrderId}", evt.OrderId);

            var notification = new
            {
                orderId = evt.OrderId,
                message = $"Order #{evt.OrderId} placed successfully! Total: {evt.TotalAmount:C}",
                placedAt = evt.PlacedAt
            };

            await _hubContext.Clients.Group(evt.CustomerId)
                .SendAsync("OrderNotification", notification);

            await _hubContext.Clients.All
                .SendAsync("OrderUpdate", notification);
        }
    }
}