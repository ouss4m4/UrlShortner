using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UrlShortner.API.Services;

namespace UrlShortner.API.Controllers
{
    // Analytics are read-only, computed from Visit events
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        // GET api/analytics/url/{urlId}
        [HttpGet("url/{urlId}")]
        public async Task<IActionResult> GetUrlAnalytics(int urlId)
        {
            var analytics = await _analyticsService.GetUrlAnalyticsAsync(urlId);
            return Ok(analytics);
        }

        // GET api/analytics/date?start=2026-01-01&end=2026-01-31
        [HttpGet("date")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (start > end)
            {
                return BadRequest(new { error = "Start date must be before end date" });
            }

            // Ensure dates are treated as UTC for PostgreSQL compatibility
            var startUtc = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(end, DateTimeKind.Utc);

            var analytics = await _analyticsService.GetAnalyticsByDateRangeAsync(startUtc, endUtc);
            return Ok(analytics);
        }
    }
}
