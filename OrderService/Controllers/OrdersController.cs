using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Events;
using OrderService.Models;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(OrderDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        // GET api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAll()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ToListAsync();
            return Ok(orders.Select(o => MapToDto(o)));
        }

        // GET api/orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return Ok(MapToDto(order));
        }

        // GET api/orders/customer/customer123
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetByCustomer(string customerId)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
            return Ok(orders.Select(o => MapToDto(o)));
        }

        // POST api/orders
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> Create(CreateOrderDto dto)
        {
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                CustomerEmail = dto.CustomerEmail,
                Items = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList(),
                TotalAmount = dto.Items.Sum(i => i.Quantity * i.UnitPrice),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Publish event to RabbitMQ
            await _publishEndpoint.Publish(new OrderPlacedEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                CustomerEmail = order.CustomerEmail,
                TotalAmount = order.TotalAmount,
                PlacedAt = order.CreatedAt
            });

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, MapToDto(order));
        }

        // PUT api/orders/5/status
        [HttpPut("{id}/status")]
        public async Task<ActionResult<OrderResponseDto>> UpdateStatus(int id, [FromBody] string status)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (Enum.TryParse<OrderStatus>(status, out var newStatus))
                order.Status = newStatus;
            else
                return BadRequest("Invalid status");

            await _context.SaveChangesAsync();
            return Ok(MapToDto(order));
        }

        // DELETE api/orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static OrderResponseDto MapToDto(Order o) => new()
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            CustomerEmail = o.CustomerEmail,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            TotalAmount = o.TotalAmount,
            Status = o.Status.ToString(),
            CreatedAt = o.CreatedAt
        };
    }
}