using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortner.DTOs;
using UrlShortner.Models;

namespace UrlShortner.API.Services
{
    public interface IUrlService
    {
        Task<Url> CreateUrlAsync(Url url);
        Task<BulkCreateResult> BulkCreateUrlsAsync(IEnumerable<Url> urls);
        Task<Url?> GetUrlByIdAsync(int id);
        Task<Url?> GetUrlByShortCodeAsync(string shortCode);
        Task<IEnumerable<Url>> GetUrlsByUserIdAsync(int userId);
        Task<IEnumerable<Url>> GetUrlsByCategoryAsync(string category, int userId);
        Task<IEnumerable<Url>> GetUrlsByTagAsync(string tag, int userId);
        Task<Url?> UpdateUrlAsync(Url url);
        Task<bool> DeleteUrlAsync(int id);
    }
}
