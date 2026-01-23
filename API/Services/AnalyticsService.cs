using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.API.Models;

namespace UrlShortner.API.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly UrlShortnerDbContext _context;
        public AnalyticsService(UrlShortnerDbContext context)
        {
            _context = context;
        }

        public async Task<Analytics> CreateAnalyticsAsync(Analytics analytics)
        {
            _context.Analytics.Add(analytics);
            await _context.SaveChangesAsync();
            return analytics;
        }

        public async Task<Analytics?> GetAnalyticsByIdAsync(int id)
        {
            return await _context.Analytics.FindAsync(id);
        }

        public async Task<IEnumerable<Analytics>> GetAnalyticsByDateAsync(System.DateTime date)
        {
            return await _context.Analytics.Where(a => a.StatDate.Date == date.Date).ToListAsync();
        }

        public async Task<IEnumerable<Analytics>> GetAnalyticsByCountryAsync(string country)
        {
            return await _context.Analytics.Where(a => a.Country == country).ToListAsync();
        }

        public async Task<Analytics?> UpdateAnalyticsAsync(Analytics analytics)
        {
            var existing = await _context.Analytics.FindAsync(analytics.Id);
            if (existing == null) return null;
            _context.Entry(existing).CurrentValues.SetValues(analytics);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAnalyticsAsync(int id)
        {
            var analytics = await _context.Analytics.FindAsync(id);
            if (analytics == null) return false;
            _context.Analytics.Remove(analytics);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
