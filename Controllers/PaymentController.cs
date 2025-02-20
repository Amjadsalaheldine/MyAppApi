using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAppApi.Data;
using MyAppApi.Models;

namespace MyAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> PostPayment(PaymentDto paymentDto)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == paymentDto.BookingId);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            decimal totalPaid = booking.Payments.Sum(p => p.Amount);
            decimal remainingBalance = booking.TotalPrice - totalPaid - paymentDto.Amount;

            if (remainingBalance < 0)
            {
                return BadRequest("The paid amount exceeds the remaining balance.");
            }

            var payment = new Payment
            {
                Amount = paymentDto.Amount,
                PaymentDate = DateTime.Now,
                RemainingBalance = remainingBalance,
                BookingId = paymentDto.BookingId
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayments), new { bookingId = payment.BookingId }, new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                RemainingBalance = payment.RemainingBalance,
                BookingId = payment.BookingId
            });
        }

        [HttpGet("{bookingId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            var paymentDtos = booking.Payments.Select(payment => new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                RemainingBalance = payment.RemainingBalance,
                BookingId = payment.BookingId,
                TotalPrice = booking.TotalPrice
            }).ToList();

            return Ok(paymentDtos);
        }

        [HttpGet("receipt/{paymentId}")]
        public async Task<ActionResult> PrintReceipt(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return NotFound("Payment not found.");
            }

            return Ok(new
            {
                PaymentId = payment.Id,
                BookingId = payment.BookingId,
                AmountPaid = payment.Amount,
                PaymentDate = payment.PaymentDate,
                RemainingBalance = payment.RemainingBalance,
                TotalPrice = payment.Booking.TotalPrice,
                StartDate = payment.Booking.StartDate,
                EndDate = payment.Booking.EndDate
            });
        }
    }
}
