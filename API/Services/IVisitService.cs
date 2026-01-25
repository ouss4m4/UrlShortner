using System.Threading.Tasks;
using UrlShortner.API.Models;

namespace UrlShortner.API.Services
{
    // Visits are events, not CRUD resources - only CreateVisitAsync is needed
    public interface IVisitService
    {
        Task<Visit> CreateVisitAsync(Visit visit);
    }
}
