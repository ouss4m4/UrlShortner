using System.Threading.Tasks;
using UrlShortner.API.Data;
using UrlShortner.Models;

namespace UrlShortner.API.Services
{
    // Visits are events - only need CreateVisitAsync for tracking
    public class VisitService : IVisitService
    {
        private readonly UrlShortnerDbContext _context;

        public VisitService(UrlShortnerDbContext context)
        {
            _context = context;
        }

        public async Task<Visit> CreateVisitAsync(Visit visit)
        {
            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();
            return visit;
        }
    }
}
