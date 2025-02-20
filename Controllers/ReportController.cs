using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAppApi.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyAppApi.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("generate/{bookingId}")]
        public async Task<IActionResult> GenerateReport(int bookingId)
        {
            
            string reportPath = Path.Combine(Directory.GetCurrentDirectory(),"fastreport.frx");

            if (!System.IO.File.Exists(reportPath))
            {
                return NotFound("Report file not found.");
            }

            var booking = await _context.Bookings
                .Where(b => b.Id == bookingId)
                .Select(b => new
                {
                    b.Id,
                    UserName = b.User.UserName,
                    CarModel = b.Car.Model,
                    b.StartDate,
                    b.EndDate,
                    b.TotalPrice
                })
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            
            var bookingData = new BookingReportData
            {
                Id = booking.Id,
                UserName = booking.UserName,
                CarModel = booking.CarModel,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                TotalPrice = booking.TotalPrice
            };

            Report report = new();
            report.Load(reportPath);
            report.RegisterData(new List<BookingReportData> { bookingData }, "Booking");
            report.Prepare();

           
            using MemoryStream pdfStream = new();
            PDFSimpleExport pdfExport = new();
            report.Export(pdfExport, pdfStream);
            pdfStream.Position = 0;

            return this.File(pdfStream.ToArray(), "application/pdf", $"Booking_{bookingId}.pdf");
        }
    }

    
    public class BookingReportData
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string CarModel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
