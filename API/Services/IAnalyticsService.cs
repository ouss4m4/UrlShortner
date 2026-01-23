using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortner.API.Models;

namespace UrlShortner.API.Services
{
    public interface IAnalyticsService
    {
        Task<Analytics> CreateAnalyticsAsync(Analytics analytics);
        Task<Analytics?> GetAnalyticsByIdAsync(int id);
        Task<IEnumerable<Analytics>> GetAnalyticsByDateAsync(System.DateTime date);
        Task<IEnumerable<Analytics>> GetAnalyticsByCountryAsync(string country);
        Task<Analytics?> UpdateAnalyticsAsync(Analytics analytics);
        Task<bool> DeleteAnalyticsAsync(int id);
    }
}
