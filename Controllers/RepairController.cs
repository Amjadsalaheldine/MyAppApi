using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAppApi.Data;
using MyAppApi.Data.Dtos;

using MyAppApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RepairController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Car/{carId}")]
        public async Task<ActionResult<IEnumerable<RepairDto>>> GetRepairsByCarId(int carId)
        {
            var repairs = await _context.Repairs
                .Where(r => r.CarId == carId)
                .Include(r => r.Workshop)
                .Include(r => r.Payments) 
                .Select(r => new RepairDto
                {
                    Id = r.Id,
                    CarId = r.CarId,
                    WorkshopId = r.WorkshopId,
                    DeliveryDate = r.DeliveryDate,
                    ReceiveDate = r.ReceiveDate,
                    Problem = r.Problem,
                    Memo = r.Memo,
                    Payment = r.Payments.Sum(p => p.Amount), 
                    Responsible = r.Workshop.Responsible, 
                    Workshop = new WorkshopDto
                    {
                        Id = r.Workshop.Id,
                        Name = r.Workshop.Name,
                        Responsible = r.Workshop.Responsible
                    },
                    Payments = r.Payments.Select(p => new PaymentDto
                    {
                        Id = p.Id,
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        PaymentMethod = p.PaymentMethod,
                        Reference = p.Reference,
                        Note = p.Note,
                        PaymentType = p.PaymentType
                    }).ToList()
                })
                .ToListAsync();

            return Ok(repairs);
        }

       
        [HttpPost]
        public async Task<ActionResult<RepairDto>> CreateRepair([FromBody] CreateRepairDto createRepairDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

         
            var car = await _context.Cars.FindAsync(createRepairDto.CarId);
            if (car == null)
            {
                return NotFound("Car not found");
            }

            
            var workshop = await _context.Workshops.FindAsync(createRepairDto.Workshop.Id);
            if (workshop == null)
            {
                workshop = new Workshop
                {
                    Name = createRepairDto.Workshop.Name,
                    Responsible = createRepairDto.Workshop.Responsible
                };
                _context.Workshops.Add(workshop);
                await _context.SaveChangesAsync();
            }

          
            var repair = new Repair
            {
                CarId = createRepairDto.CarId,
                WorkshopId = workshop.Id,
                DeliveryDate = createRepairDto.DeliveryDate,
                ReceiveDate = createRepairDto.ReceiveDate,
                Problem = createRepairDto.Problem,
                Memo = createRepairDto.Memo
            };

            _context.Repairs.Add(repair);
            await _context.SaveChangesAsync();

          
            var payment = new Payment
            {
                Amount = createRepairDto.Payment,
                PaymentDate = DateTime.UtcNow,
                RemainingBalance = 0,
                PaymentMethod = "Cash",
                Reference = "Repair Payment",
                Note = "Payment for repair",
                PaymentType = (Models.PaymentType)PaymentType.Pay,
                RepairId = repair.Id
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            
            var repairDto = new RepairDto
            {
                Id = repair.Id,
                CarId = repair.CarId,
                WorkshopId = repair.WorkshopId,
                DeliveryDate = repair.DeliveryDate,
                ReceiveDate = repair.ReceiveDate,
                Problem = repair.Problem,
                Memo = repair.Memo,
                Responsible = workshop.Responsible,
                Workshop = new WorkshopDto
                {
                    Id = workshop.Id,
                    Name = workshop.Name,
                    Responsible = workshop.Responsible
                }
            };

            return CreatedAtAction(nameof(GetRepairsByCarId), new { carId = repair.CarId }, repairDto);
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRepair(int id, [FromBody] UpdateRepairDto updateRepairDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var repair = await _context.Repairs
                .Include(r => r.Payments) 
                .FirstOrDefaultAsync(r => r.Id == id);

            if (repair == null)
            {
                return NotFound("Repair not found");
            }

     
            repair.DeliveryDate = updateRepairDto.DeliveryDate;
            repair.ReceiveDate = updateRepairDto.ReceiveDate;
            repair.Problem = updateRepairDto.Problem;
            repair.Memo = updateRepairDto.Memo;

       
            if (updateRepairDto.Payments != null && updateRepairDto.Payments.Any())
            {
               
                _context.Payments.RemoveRange(repair.Payments);

              
                foreach (var paymentDto in updateRepairDto.Payments)
                {
                    var payment = new Payment
                    {
                        Amount = paymentDto.Amount,
                        PaymentDate = paymentDto.PaymentDate,
                        PaymentMethod = paymentDto.PaymentMethod,
                        Reference = paymentDto.Reference,
                        Note = paymentDto.Note,
                        PaymentType = paymentDto.PaymentType,
                        RepairId = repair.Id 
                    };
                    _context.Payments.Add(payment);
                }
            }

            _context.Repairs.Update(repair);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRepair(int id)
        {
         
            var repair = await _context.Repairs
                .Include(r => r.Payments) 
                .FirstOrDefaultAsync(r => r.Id == id);

            if (repair == null)
            {
                return NotFound("Repair not found");
            }

            
            _context.Payments.RemoveRange(repair.Payments);

            
            _context.Repairs.Remove(repair);

            
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
