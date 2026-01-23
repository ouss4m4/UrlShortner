using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortner.API.Models;

namespace UrlShortner.API.Services
{
    public interface IUrlService
    {
        Task<Url> CreateUrlAsync(Url url);
        Task<Url?> GetUrlByIdAsync(int id);
        Task<Url?> GetUrlByShortCodeAsync(string shortCode);
        Task<IEnumerable<Url>> GetUrlsByUserIdAsync(int userId);
        Task<Url?> UpdateUrlAsync(Url url);
        Task<bool> DeleteUrlAsync(int id);
    }
}
