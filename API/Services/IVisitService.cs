using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortner.API.Models;

namespace UrlShortner.API.Services
{
    public interface IVisitService
    {
        Task<Visit> CreateVisitAsync(Visit visit);
        Task<Visit?> GetVisitByIdAsync(int id);
        Task<IEnumerable<Visit>> GetVisitsByUrlIdAsync(int urlId);
        Task<IEnumerable<Visit>> GetVisitsByUserIdAsync(int userId);
        Task<Visit?> UpdateVisitAsync(Visit visit);
        Task<bool> DeleteVisitAsync(int id);
    }
}
