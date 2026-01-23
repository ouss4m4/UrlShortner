using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.API.Models;

namespace UrlShortner.API.Services
{
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

        public async Task<Visit?> GetVisitByIdAsync(int id)
        {
            return await _context.Visits.FindAsync(id);
        }

        public async Task<IEnumerable<Visit>> GetVisitsByUrlIdAsync(int urlId)
        {
            return await _context.Visits.Where(v => v.UrlId == urlId).ToListAsync();
        }

        public async Task<IEnumerable<Visit>> GetVisitsByUserIdAsync(int userId)
        {
            return await _context.Visits.Where(v => v.UserId == userId).ToListAsync();
        }

        public async Task<Visit?> UpdateVisitAsync(Visit visit)
        {
            var existing = await _context.Visits.FindAsync(visit.Id);
            if (existing == null) return null;
            _context.Entry(existing).CurrentValues.SetValues(visit);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteVisitAsync(int id)
        {
            var visit = await _context.Visits.FindAsync(id);
            if (visit == null) return false;
            _context.Visits.Remove(visit);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
