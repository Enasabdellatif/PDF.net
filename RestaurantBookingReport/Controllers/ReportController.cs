using Microsoft.AspNetCore.Mvc;
using RestaurantBookingReport.Services;
using RestaurantBookingReport.Models;
using System;
using System.Threading.Tasks;

namespace RestaurantBookingReport.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly RestaurantService _restaurantService;
        private readonly PdfService _pdfService;

        public ReportController(RestaurantService restaurantService, PdfService pdfService)
        {
            _restaurantService = restaurantService;
            _pdfService = pdfService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequest request)
        {
            try
            {
                var report = await _restaurantService.GetRestaurantReportAsync(request.RestaurantId, request.Date);
                var pdfStream = _pdfService.GeneratePdf(report, request.Date);

                return File(pdfStream.ToArray(), "application/pdf", "report.pdf");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }

    public class ReportRequest
    {
        public int RestaurantId { get; set; }
        public DateTime Date { get; set; }
    }
}
