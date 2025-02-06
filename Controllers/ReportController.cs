using FastReport;
using FastReport.Export.PdfSimple;
using FastReport.Data;
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
            MsSqlDataConnection.Register();

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
                return NotFound("Booking Not Found");
            }

            string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "fastreport.frx");
            if (!System.IO.File.Exists(reportPath))
            {
                return NotFound("reports not found .");
            }

            Report report = new();
            report.Load(reportPath);
            report.RegisterData(new List<object> { booking }, "Booking");
            report.Prepare();

            using MemoryStream pdfStream = new();
            PDFSimpleExport pdfExport = new();
            report.Export(pdfExport, pdfStream);
            pdfStream.Position = 0;

            return this.File(pdfStream.ToArray(), "application/pdf", $"Booking_{bookingId}.pdf");
        }
    }
}
