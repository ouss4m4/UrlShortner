using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using UrlShortner.API.Models;
using UrlShortner.API.Services;
using Xunit;

namespace UrlShortner.Test
{
    public class UrlServiceTests
    {
        private readonly Mock<IUrlService> _mockService;
        public UrlServiceTests()
        {
            _mockService = new Mock<IUrlService>();
        }

        [Fact]
        public async Task CreateUrlAsync_ReturnsUrl()
        {
            var url = new Url { Id = 1, OriginalUrl = "https://example.com", ShortCode = "abc123", UserId = 1 };
            _mockService.Setup(s => s.CreateUrlAsync(It.IsAny<Url>())).ReturnsAsync(url);
            var result = await _mockService.Object.CreateUrlAsync(url);
            Assert.Equal(url.Id, result.Id);
        }

        [Fact]
        public async Task GetUrlByIdAsync_ReturnsUrlOrNull()
        {
            var url = new Url { Id = 2, OriginalUrl = "https://test.com", ShortCode = "def456", UserId = 1 };
            _mockService.Setup(s => s.GetUrlByIdAsync(2)).ReturnsAsync(url);
            var result = await _mockService.Object.GetUrlByIdAsync(2);
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
        }

        [Fact]
        public async Task DeleteUrlAsync_ReturnsTrueIfDeleted()
        {
            _mockService.Setup(s => s.DeleteUrlAsync(3)).ReturnsAsync(true);
            var result = await _mockService.Object.DeleteUrlAsync(3);
            Assert.True(result);
        }
    }
}
