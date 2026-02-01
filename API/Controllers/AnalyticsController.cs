using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        private readonly IUrlService _urlService;

        public AnalyticsController(IAnalyticsService analyticsService, IUrlService urlService)
        {
            _analyticsService = analyticsService;
            _urlService = urlService;
        }

        // GET api/analytics/url/{urlId}
        [Authorize]
        [HttpGet("url/{urlId}")]
        public async Task<IActionResult> GetUrlAnalytics(int urlId)
        {
            // Verify URL ownership before showing analytics
            var url = await _urlService.GetUrlByIdAsync(urlId);
            if (url == null) return NotFound();

            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserIdClaim == null || url.UserId != int.Parse(currentUserIdClaim))
            {
                return Forbid();
            }

            var analytics = await _analyticsService.GetUrlAnalyticsAsync(urlId);
            return Ok(analytics);
        }

        // GET api/analytics/date?start=2026-01-01&end=2026-01-31
        [Authorize]
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

        // GET api/analytics/country
        [Authorize]
        [HttpGet("country")]
        public async Task<IActionResult> GetByCountry()
        {
            var analytics = await _analyticsService.GetAnalyticsByCountryAsync();
            return Ok(analytics);
        }
    }
}
