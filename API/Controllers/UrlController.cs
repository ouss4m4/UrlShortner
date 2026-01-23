using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UrlShortner.API.Models;
using UrlShortner.API.Services;

namespace UrlShortner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly IUrlService _urlService;
        public UrlController(IUrlService urlService)
        {
            _urlService = urlService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Url url)
        {
            try
            {
                var created = await _urlService.CreateUrlAsync(url);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                // Return 409 Conflict with a clear error message for UI
                return Conflict(new
                {
                    error = "ShortCodeAlreadyExists",
                    message = ex.Message,
                    shortCode = url.ShortCode
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var url = await _urlService.GetUrlByIdAsync(id);
            if (url == null) return NotFound();
            return Ok(url);
        }

        [HttpGet("short/{shortCode}")]
        public async Task<IActionResult> GetByShortCode(string shortCode)
        {
            var url = await _urlService.GetUrlByShortCodeAsync(shortCode);
            if (url == null) return NotFound();
            return Ok(url);
        }

        [HttpGet("redirect/{shortCode}")]
        public async Task<IActionResult> RedirectToOriginal(string shortCode)
        {
            var url = await _urlService.GetUrlByShortCodeAsync(shortCode);
            if (url == null) return NotFound();

            // TODO: Track visit (record IP, browser, country, etc.)

            return Redirect(url.OriginalUrl);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var urls = await _urlService.GetUrlsByUserIdAsync(userId);
            return Ok(urls);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Url url)
        {
            if (id != url.Id) return BadRequest();
            var updated = await _urlService.UpdateUrlAsync(url);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _urlService.DeleteUrlAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
