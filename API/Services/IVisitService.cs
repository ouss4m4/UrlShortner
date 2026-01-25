using System.Threading.Tasks;
using UrlShortner.Models;

namespace UrlShortner.API.Services
{
    // Visits are events, not CRUD resources - only CreateVisitAsync is needed
    public interface IVisitService
    {
        Task<Visit> CreateVisitAsync(Visit visit);
    }
}
