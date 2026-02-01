using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using UrlShortner.Models;
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
            // If custom short code is provided, require authentication
            if (!string.IsNullOrWhiteSpace(url.ShortCode))
            {
                var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new
                    {
                        error = "AuthenticationRequired",
                        message = "Custom short codes (aliases) require authentication. Anonymous users can only use auto-generated short codes."
                    });
                }
            }

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

        // Note: Redirect moved to root-level endpoint in Program.cs: /{shortCode}
        // This keeps URLs short (the whole point!) instead of /api/url/redirect/{shortCode}

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            // Only allow users to access their own URLs
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserIdClaim == null || int.Parse(currentUserIdClaim) != userId)
            {
                return Forbid();
            }

            var urls = await _urlService.GetUrlsByUserIdAsync(userId);
            return Ok(urls);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Url url)
        {
            if (id != url.Id) return BadRequest();

            // Verify ownership before updating
            var existingUrl = await _urlService.GetUrlByIdAsync(id);
            if (existingUrl == null) return NotFound();

            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserIdClaim == null || existingUrl.UserId != int.Parse(currentUserIdClaim))
            {
                return Forbid();
            }

            var updated = await _urlService.UpdateUrlAsync(url);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Verify ownership before deleting
            var existingUrl = await _urlService.GetUrlByIdAsync(id);
            if (existingUrl == null) return NotFound();

            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserIdClaim == null || existingUrl.UserId != int.Parse(currentUserIdClaim))
            {
                return Forbid();
            }

            var deleted = await _urlService.DeleteUrlAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
