using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Events;
using PaymentService.Models;

namespace PaymentService.Consumers
{
    public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly PaymentDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<OrderPlacedConsumer> _logger;

        public OrderPlacedConsumer(PaymentDbContext context, IPublishEndpoint publishEndpoint, ILogger<OrderPlacedConsumer> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var orderEvent = context.Message;
            _logger.LogInformation("Processing payment for Order {OrderId}, Amount: {Amount}", orderEvent.OrderId, orderEvent.TotalAmount);

            // Create payment record
            var payment = new Payment
            {
                OrderId = orderEvent.OrderId,
                CustomerId = orderEvent.CustomerId,
                CustomerEmail = orderEvent.CustomerEmail,
                Amount = orderEvent.TotalAmount,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Simulate payment processing (80% success rate)
            await Task.Delay(1000);
            var random = new Random();
            var isSuccessful = random.Next(1, 101) <= 80;

            payment.Status = isSuccessful ? PaymentStatus.Succeeded : PaymentStatus.Failed;
            payment.ProcessedAt = DateTime.UtcNow;
            payment.FailureReason = isSuccessful ? null : "Insufficient funds";

            await _context.SaveChangesAsync();

            // Publish result back to RabbitMQ
            await _publishEndpoint.Publish(new PaymentProcessedEvent
            {
                OrderId = orderEvent.OrderId,
                CustomerId = orderEvent.CustomerId,
                CustomerEmail = orderEvent.CustomerEmail,
                Amount = orderEvent.TotalAmount,
                IsSuccessful = isSuccessful,
                FailureReason = payment.FailureReason,
                ProcessedAt = payment.ProcessedAt.Value
            });

            _logger.LogInformation("Payment for Order {OrderId} {Status}", orderEvent.OrderId, isSuccessful ? "SUCCEEDED" : "FAILED");
        }
    }
}