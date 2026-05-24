namespace OrderService.Events
{
    public class OrderPlacedEvent
    {
        public int OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime PlacedAt { get; set; }
    }
}