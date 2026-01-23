using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.API.Models;

namespace UrlShortner.API.Services
{
    public class UrlService : IUrlService
    {
        private readonly UrlShortnerDbContext _context;
        public UrlService(UrlShortnerDbContext context)
        {
            _context = context;
        }

        public async Task<Url> CreateUrlAsync(Url url)
        {
            _context.Urls.Add(url);
            await _context.SaveChangesAsync();
            return url;
        }

        public async Task<Url?> GetUrlByIdAsync(int id)
        {
            return await _context.Urls.FindAsync(id);
        }

        public async Task<Url?> GetUrlByShortCodeAsync(string shortCode)
        {
            return await _context.Urls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
        }

        public async Task<IEnumerable<Url>> GetUrlsByUserIdAsync(int userId)
        {
            return await _context.Urls.Where(u => u.UserId == userId).ToListAsync();
        }

        public async Task<Url?> UpdateUrlAsync(Url url)
        {
            var existing = await _context.Urls.FindAsync(url.Id);
            if (existing == null) return null;
            _context.Entry(existing).CurrentValues.SetValues(url);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteUrlAsync(int id)
        {
            var url = await _context.Urls.FindAsync(id);
            if (url == null) return false;
            _context.Urls.Remove(url);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
