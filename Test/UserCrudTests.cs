using Xunit;
using UrlShortner.Models;
using UrlShortner.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Test
{
    public class UserCrudTests
    {
        private UrlShortnerDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<UrlShortnerDbContext>()
                .UseInMemoryDatabase(databaseName: "UserCrudTestDb")
                .Options;
            return new UrlShortnerDbContext(options);
        }

        [Fact]
        public async Task CanCreateUser()
        {
            var db = GetDbContext();
            var user = new User { Username = "testuser", Email = "test@example.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            Assert.NotEqual(0, user.Id);
        }

        [Fact]
        public async Task CanReadUser()
        {
            var db = GetDbContext();
            var user = new User { Username = "readuser", Email = "read@example.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var found = await db.Users.FindAsync(user.Id);
            Assert.NotNull(found);
            Assert.Equal("readuser", found.Username);
        }

        [Fact]
        public async Task CanUpdateUser()
        {
            var db = GetDbContext();
            var user = new User { Username = "updateuser", Email = "update@example.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            user.Email = "updated@example.com";
            db.Users.Update(user);
            await db.SaveChangesAsync();
            var found = await db.Users.FindAsync(user.Id);
            Assert.NotNull(found);
            Assert.Equal("updated@example.com", found.Email);
        }

        [Fact]
        public async Task CanDeleteUser()
        {
            var db = GetDbContext();
            var user = new User { Username = "deleteuser", Email = "delete@example.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            var found = await db.Users.FindAsync(user.Id);
            Assert.Null(found);
        }
    }
}
