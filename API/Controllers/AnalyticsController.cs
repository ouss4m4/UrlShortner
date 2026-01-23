using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UrlShortner.API.Models;
using UrlShortner.API.Services;

namespace UrlShortner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Analytics analytics)
        {
            var created = await _analyticsService.CreateAnalyticsAsync(analytics);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var analytics = await _analyticsService.GetAnalyticsByIdAsync(id);
            if (analytics == null) return NotFound();
            return Ok(analytics);
        }

        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetByDate(System.DateTime date)
        {
            var analytics = await _analyticsService.GetAnalyticsByDateAsync(date);
            return Ok(analytics);
        }

        [HttpGet("country/{country}")]
        public async Task<IActionResult> GetByCountry(string country)
        {
            var analytics = await _analyticsService.GetAnalyticsByCountryAsync(country);
            return Ok(analytics);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Analytics analytics)
        {
            if (id != analytics.Id) return BadRequest();
            var updated = await _analyticsService.UpdateAnalyticsAsync(analytics);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _analyticsService.DeleteAnalyticsAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
