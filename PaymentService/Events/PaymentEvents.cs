namespace PaymentService.Events
{
    public class OrderPlacedEvent
    {
        public int OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime PlacedAt { get; set; }
    }

    public class PaymentProcessedEvent
    {
        public int OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}