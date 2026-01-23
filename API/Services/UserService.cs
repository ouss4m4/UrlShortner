using UrlShortner.API.Models;
using UrlShortner.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UrlShortner.API.Services
{
    public class UserService : IUserService
    {
        private readonly UrlShortnerDbContext _db;
        public UserService(UrlShortnerDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> GetAllAsync() => await _db.Users.ToListAsync();

        public async Task<User?> GetByIdAsync(int id) => await _db.Users.FindAsync(id);

        public async Task<User> CreateAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateAsync(int id, User user)
        {
            var existing = await _db.Users.FindAsync(id);
            if (existing == null) return null;
            existing.Username = user.Username;
            existing.Email = user.Email;
            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return false;
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
