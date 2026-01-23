using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UrlShortner.API.Models;
using UrlShortner.API.Services;

namespace UrlShortner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitController : ControllerBase
    {
        private readonly IVisitService _visitService;
        public VisitController(IVisitService visitService)
        {
            _visitService = visitService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Visit visit)
        {
            var created = await _visitService.CreateVisitAsync(visit);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var visit = await _visitService.GetVisitByIdAsync(id);
            if (visit == null) return NotFound();
            return Ok(visit);
        }

        [HttpGet("url/{urlId}")]
        public async Task<IActionResult> GetByUrlId(int urlId)
        {
            var visits = await _visitService.GetVisitsByUrlIdAsync(urlId);
            return Ok(visits);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var visits = await _visitService.GetVisitsByUserIdAsync(userId);
            return Ok(visits);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Visit visit)
        {
            if (id != visit.Id) return BadRequest();
            var updated = await _visitService.UpdateVisitAsync(visit);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _visitService.DeleteVisitAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
