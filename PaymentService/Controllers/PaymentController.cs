using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentDbContext _context;

        public PaymentsController(PaymentDbContext context)
        {
            _context = context;
        }

        // GET api/payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetAll()
        {
            var payments = await _context.Payments.ToListAsync();
            return Ok(payments.Select(p => MapToDto(p)));
        }

        // GET api/payments/order/5
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<PaymentResponseDto>> GetByOrderId(int orderId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (payment == null) return NotFound();
            return Ok(MapToDto(payment));
        }

        // GET api/payments/customer/customer123
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetByCustomer(string customerId)
        {
            var payments = await _context.Payments
                .Where(p => p.CustomerId == customerId)
                .ToListAsync();
            return Ok(payments.Select(p => MapToDto(p)));
        }

        private static PaymentResponseDto MapToDto(Payment p) => new()
        {
            Id = p.Id,
            OrderId = p.OrderId,
            CustomerId = p.CustomerId,
            CustomerEmail = p.CustomerEmail,
            Amount = p.Amount,
            Status = p.Status.ToString(),
            FailureReason = p.FailureReason,
            CreatedAt = p.CreatedAt,
            ProcessedAt = p.ProcessedAt
        };
    }
}