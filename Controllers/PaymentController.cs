using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAppApi.Data;
using MyAppApi.Data.Dtos;
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
        [HttpGet("stats")]
        public async Task<ActionResult> GetPaymentStats([FromQuery] string period)
        {
            DateTime startDate;
            DateTime endDate = DateTime.Now;

            switch (period.ToLower())
            {
                case "weekly":
                    startDate = endDate.AddDays(-7);
                    break;
                case "monthly":
                    startDate = endDate.AddMonths(-1);
                    break;
                case "yearly":
                    startDate = endDate.AddYears(-1);
                    break;
                default:
                    return BadRequest("Invalid period. Please specify 'weekly', 'monthly', or 'yearly'.");
            }

            var payments = await _context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .Select(p => new
                {
                    p.Amount,
                    p.PaymentDate,
                    p.PaymentMethod,
                    p.Reference,
                    p.Note,
                    p.PaymentType
                })
                .ToListAsync();

            var totalPay = payments.Where(p => p.PaymentType == (Models.PaymentType)PaymentType.Pay).Sum(p => p.Amount);
            var totalReceive = payments.Where(p => p.PaymentType == (Models.PaymentType)PaymentType.Receive).Sum(p => p.Amount);

            var totalPaymentsPay = payments.Count(p => p.PaymentType == (Models.PaymentType)PaymentType.Pay);
            var totalPaymentsReceive = payments.Count(p => p.PaymentType == (Models.PaymentType)PaymentType.Receive);

            var totalAmount = totalReceive - totalPay;

            return Ok(new
            {
                TotalPayments = payments.Count,
                TotalAmount = totalAmount,
                TotalPay = totalPay,
                TotalReceive = totalReceive,
                TotalPaymentsPay = totalPaymentsPay,
                TotalPaymentsReceive = totalPaymentsReceive,
                Payments = payments
            });
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> PostPayment([FromBody] PaymentDto paymentDto)
        {
            if (paymentDto == null)
            {
                return BadRequest("Payment data is required.");
            }

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
                BookingId = paymentDto.BookingId,
                PaymentMethod = paymentDto.PaymentMethod,
                Reference = paymentDto.Reference,
                Note = paymentDto.Note,
                PaymentType = (Models.PaymentType)PaymentType.Receive 
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayments), new { bookingId = payment.BookingId }, new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                RemainingBalance = payment.RemainingBalance,
                BookingId = payment.BookingId ?? 0,
                PaymentMethod = payment.PaymentMethod,
                Reference = payment.Reference,
                Note = payment.Note,
                PaymentType = payment.PaymentType 
            });
        }

        [HttpGet("{bookingId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments(int bookingId)
        {
         
            var booking = await _context.Bookings
                .Include(b => b.Payments)
                .Include(b => b.User)
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
                BookingId = payment.BookingId ?? 0,
                TotalPrice = booking.TotalPrice,
                PaymentMethod = payment.PaymentMethod,
                Reference = payment.Reference,
                Note = payment.Note,
                UserName = booking.User.UserName,
                PaymentType = payment.PaymentType 
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
                EndDate = payment.Booking.EndDate,
                PaymentMethod = payment.PaymentMethod, 
                Reference = payment.Reference, 
                Note = payment.Note 
            });
        }

        [HttpPut("{paymentId}")]
        public async Task<IActionResult> UpdatePayment(int paymentId, PaymentDto paymentDto)
        {
            if (paymentDto == null)
            {
                return BadRequest("Payment data is required.");
            }

            var payment = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.User) 
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return NotFound("Payment not found.");
            }

            var booking = payment.Booking;
            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

          
            paymentDto.UserName = booking.User.UserName;

           
            decimal totalPaidBefore = booking.Payments.Sum(p => p.Amount);

           
            decimal totalPaidAfter = totalPaidBefore - payment.Amount + paymentDto.Amount;

            
            decimal remainingBalance = booking.TotalPrice - totalPaidAfter;

            if (remainingBalance < 0)
            {
                return BadRequest("The updated amount exceeds the total price.");
            }

          
            payment.Amount = paymentDto.Amount;
            payment.PaymentDate = paymentDto.PaymentDate;
            payment.RemainingBalance = remainingBalance;
            payment.PaymentMethod = paymentDto.PaymentMethod;
            payment.Reference = paymentDto.Reference;
            payment.Note = paymentDto.Note;
            payment.PaymentType = paymentDto.PaymentType; 

            await _context.SaveChangesAsync();

            
            return Ok(new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                RemainingBalance = payment.RemainingBalance,
                BookingId = payment.BookingId ?? 0,
                PaymentMethod = payment.PaymentMethod,
                Reference = payment.Reference,
                Note = payment.Note,
                PaymentType = payment.PaymentType, 
                UserName = paymentDto.UserName 
            });
        }
        [HttpDelete("{paymentId}")]
        public async Task<IActionResult> DeletePayment(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return NotFound("Payment not found.");
            }

            var booking = payment.Booking;
            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

           
            decimal newRemainingBalance = payment.RemainingBalance + payment.Amount;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Payment deleted successfully.", NewRemainingBalance = newRemainingBalance });
        }

    }
}
