using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.DTOs;
using UrlShortner.Models;
using API.Services;

namespace UrlShortner.API.Services
{
    public class UrlService : IUrlService
    {
        private readonly UrlShortnerDbContext _context;
        private readonly IShortCodeGenerator _shortCodeGenerator;
        private readonly ICacheService? _cacheService;
        private readonly IUrlValidator _urlValidator;
        private const string CacheKeyPrefix = "url:shortcode:";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

        private static string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            url = url.Trim();
            if (!url.Contains("://", StringComparison.Ordinal))
            {
                url = $"https://{url}";
            }

            return url;
        }

        public UrlService(
            UrlShortnerDbContext context,
            IShortCodeGenerator shortCodeGenerator,
            IUrlValidator urlValidator,
            ICacheService? cacheService = null)
        {
            _context = context;
            _shortCodeGenerator = shortCodeGenerator;
            _urlValidator = urlValidator;
            _cacheService = cacheService;
        }

        private TimeSpan CalculateCacheTTL(Url url)
        {
            // If URL has no expiry, use default 1 hour TTL
            if (!url.Expiry.HasValue)
            {
                return CacheExpiration;
            }

            // Calculate time until URL expires
            var timeUntilExpiry = url.Expiry.Value - DateTime.UtcNow;

            // If already expired or expiring very soon, don't cache
            if (timeUntilExpiry <= TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            // Use shorter of: default TTL or time until expiry
            return timeUntilExpiry < CacheExpiration ? timeUntilExpiry : CacheExpiration;
        }

        public async Task<Url> CreateUrlAsync(Url url)
        {
            try
            {
                url.OriginalUrl = NormalizeUrl(url.OriginalUrl);

                // Validate original URL
                var urlValidation = _urlValidator.ValidateUrl(url.OriginalUrl);
                if (!urlValidation.IsValid)
                {
                    throw new ArgumentException(urlValidation.ErrorMessage, nameof(url.OriginalUrl));
                }

                // Validate custom short code if provided
                if (!string.IsNullOrEmpty(url.ShortCode))
                {
                    var shortCodeValidation = _urlValidator.ValidateShortCode(url.ShortCode);
                    if (!shortCodeValidation.IsValid)
                    {
                        throw new ArgumentException(shortCodeValidation.ErrorMessage, nameof(url.ShortCode));
                    }
                }

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

                // Set CreatedAt timestamp
                if (url.CreatedAt == default)
                {
                    url.CreatedAt = DateTime.UtcNow;
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

                // Warm up cache with newly created URL
                if (_cacheService != null && !string.IsNullOrEmpty(url.ShortCode))
                {
                    var ttl = CalculateCacheTTL(url);
                    if (ttl > TimeSpan.Zero)
                    {
                        await _cacheService.SetAsync($"{CacheKeyPrefix}{url.ShortCode}", url, ttl);
                    }
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

        public async Task<BulkCreateResult> BulkCreateUrlsAsync(IEnumerable<Url> urls)
        {
            var createdUrls = new List<Url>();
            var failures = new List<BulkCreateFailure>();

            foreach (var url in urls)
            {
                try
                {
                    var created = await CreateUrlAsync(url);
                    createdUrls.Add(created);
                }
                catch (ArgumentException ex)
                {
                    // Validation error (short code too short/long, invalid characters, reserved word)
                    failures.Add(new BulkCreateFailure
                    {
                        OriginalUrl = url.OriginalUrl,
                        ShortCode = url.ShortCode ?? string.Empty,
                        Error = ex.Message
                    });
                }
                catch (InvalidOperationException ex)
                {
                    // Duplicate short code error
                    failures.Add(new BulkCreateFailure
                    {
                        OriginalUrl = url.OriginalUrl,
                        ShortCode = url.ShortCode ?? string.Empty,
                        Error = ex.Message
                    });
                }
            }

            return new BulkCreateResult
            {
                SuccessCount = createdUrls.Count,
                CreatedUrls = createdUrls,
                Failures = failures
            };
        }

        public async Task<Url?> GetUrlByIdAsync(int id)
        {
            return await _context.Urls.FindAsync(id);
        }

        public async Task<Url?> GetUrlByShortCodeAsync(string shortCode)
        {
            // Try to get from cache first
            if (_cacheService != null)
            {
                var cacheKey = $"{CacheKeyPrefix}{shortCode}";
                var cached = await _cacheService.GetAsync<Url>(cacheKey);

                if (cached != null)
                {
                    // Check if cached URL is expired
                    if (cached.Expiry.HasValue && cached.Expiry.Value <= DateTime.UtcNow)
                    {
                        // Remove expired URL from cache
                        await _cacheService.RemoveAsync(cacheKey);
                        return null;
                    }
                    return cached;
                }

                // Not in cache, get from DB
                var url = await _context.Urls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);

                // Check if URL is expired
                if (url != null && url.Expiry.HasValue && url.Expiry.Value <= DateTime.UtcNow)
                {
                    return null; // Expired URL
                }

                // Cache for future requests (only if not expired)
                if (url != null)
                {
                    var ttl = CalculateCacheTTL(url);
                    if (ttl > TimeSpan.Zero)
                    {
                        await _cacheService.SetAsync(cacheKey, url, ttl);
                    }
                }

                return url;
            }

            // No cache service, just query DB
            var result = await _context.Urls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            // Check if URL is expired
            if (result != null && result.Expiry.HasValue && result.Expiry.Value <= DateTime.UtcNow)
            {
                return null; // Expired URL
            }

            return result;
        }

        public async Task<IEnumerable<Url>> GetUrlsByUserIdAsync(int userId)
        {
            var urls = await _context.Urls.Where(u => u.UserId == userId).ToListAsync();

            // Filter out expired URLs
            return urls.Where(u => !u.Expiry.HasValue || u.Expiry.Value > DateTime.UtcNow);
        }

        public async Task<IEnumerable<Url>> GetUrlsByCategoryAsync(string category, int userId)
        {
            var urls = await _context.Urls
                .Where(u => u.UserId == userId && u.Category != null && u.Category.ToLower() == category.ToLower())
                .ToListAsync();

            // Filter out expired URLs
            return urls.Where(u => !u.Expiry.HasValue || u.Expiry.Value > DateTime.UtcNow);
        }

        public async Task<IEnumerable<Url>> GetUrlsByTagAsync(string tag, int userId)
        {
            var urls = await _context.Urls
                .Where(u => u.UserId == userId && u.Tags != null && u.Tags.ToLower().Contains(tag.ToLower()))
                .ToListAsync();

            // Filter out expired URLs
            return urls.Where(u => !u.Expiry.HasValue || u.Expiry.Value > DateTime.UtcNow);
        }

        public async Task<Url?> UpdateUrlAsync(Url url)
        {
            var existing = await _context.Urls.FindAsync(url.Id);
            if (existing == null) return null;

            // Invalidate old cache before updating
            if (_cacheService != null && !string.IsNullOrEmpty(existing.ShortCode))
            {
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}{existing.ShortCode}");
            }

            _context.Entry(existing).CurrentValues.SetValues(url);
            await _context.SaveChangesAsync();

            // Warm up cache with updated URL
            if (_cacheService != null && !string.IsNullOrEmpty(existing.ShortCode))
            {
                var ttl = CalculateCacheTTL(existing);
                if (ttl > TimeSpan.Zero)
                {
                    await _cacheService.SetAsync($"{CacheKeyPrefix}{existing.ShortCode}", existing, ttl);
                }
            }

            return existing;
        }

        public async Task<bool> DeleteUrlAsync(int id)
        {
            var url = await _context.Urls.FindAsync(id);
            if (url == null) return false;

            // Invalidate cache before deleting
            if (_cacheService != null && !string.IsNullOrEmpty(url.ShortCode))
            {
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}{url.ShortCode}");
            }

            _context.Urls.Remove(url);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
