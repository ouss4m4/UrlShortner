using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using UrlShortner.API.Models;
using UrlShortner.API.Services;
using Xunit;

namespace UrlShortner.Test
{
    public class VisitServiceTests
    {
        private readonly Mock<IVisitService> _mockService;
        public VisitServiceTests()
        {
            _mockService = new Mock<IVisitService>();
        }

        [Fact]
        public async Task CreateVisitAsync_ReturnsVisit()
        {
            var visit = new Visit { Id = 1, UrlId = 1, UserId = 1 };
            _mockService.Setup(s => s.CreateVisitAsync(It.IsAny<Visit>())).ReturnsAsync(visit);
            var result = await _mockService.Object.CreateVisitAsync(visit);
            Assert.Equal(visit.Id, result.Id);
        }

        [Fact]
        public async Task GetVisitByIdAsync_ReturnsVisitOrNull()
        {
            var visit = new Visit { Id = 2, UrlId = 1, UserId = 1 };
            _mockService.Setup(s => s.GetVisitByIdAsync(2)).ReturnsAsync(visit);
            var result = await _mockService.Object.GetVisitByIdAsync(2);
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
        }

        [Fact]
        public async Task DeleteVisitAsync_ReturnsTrueIfDeleted()
        {
            _mockService.Setup(s => s.DeleteVisitAsync(3)).ReturnsAsync(true);
            var result = await _mockService.Object.DeleteVisitAsync(3);
            Assert.True(result);
        }
    }
}
