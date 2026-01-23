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
        private readonly IShortCodeGenerator _shortCodeGenerator;

        public UrlService(UrlShortnerDbContext context, IShortCodeGenerator shortCodeGenerator)
        {
            _context = context;
            _shortCodeGenerator = shortCodeGenerator;
        }

        public async Task<Url> CreateUrlAsync(Url url)
        {
            try
            {
                // Check for duplicate short code if one is provided
                if (!string.IsNullOrEmpty(url.ShortCode))
                {
                    var existing = await _context.Urls
                        .FirstOrDefaultAsync(u => u.ShortCode == url.ShortCode);

                    if (existing != null)
                    {
                        throw new InvalidOperationException(
                            $"Short code '{url.ShortCode}' is already taken. Please choose a different short code."
                        );
                    }
                }

                // Add the URL first to get an ID
                _context.Urls.Add(url);
                await _context.SaveChangesAsync();

                // If no short code provided, generate one from the ID
                if (string.IsNullOrEmpty(url.ShortCode))
                {
                    url.ShortCode = _shortCodeGenerator.Encode(url.Id);
                    await _context.SaveChangesAsync();
                }

                return url;
            }
            catch (DbUpdateException ex)
            {
                // Catch any database-level unique constraint violations
                if (ex.InnerException?.Message.Contains("ShortCode") == true ||
                    ex.InnerException?.Message.Contains("IX_Urls_ShortCode") == true)
                {
                    throw new InvalidOperationException(
                        $"Short code '{url.ShortCode}' is already taken. Please choose a different short code.",
                        ex
                    );
                }
                throw; // Re-throw if it's a different DB error
            }
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
