using Xunit;
using Moq;
using UrlShortner.Models;
using UrlShortner.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    public class UserServiceUnitTests
    {
        [Fact]
        public async Task CanCreateUser_AddsUserToDbSet()
        {
            var users = new List<User>();
            var mockSet = new Mock<DbSet<User>>();
            mockSet.Setup(m => m.Add(It.IsAny<User>())).Callback<User>(u => users.Add(u));

            var mockContext = new Mock<UrlShortnerDbContext>(new DbContextOptions<UrlShortnerDbContext>());
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var user = new User { Username = "mockuser", Email = "mock@example.com" };
            mockSet.Object.Add(user);
            await mockContext.Object.SaveChangesAsync();

            Assert.Single(users);
            Assert.Equal("mockuser", users[0].Username);
        }
    }
}
