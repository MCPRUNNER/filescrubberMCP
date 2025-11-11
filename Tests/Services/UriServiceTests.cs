using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Services;
using filescrubberMCP.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

namespace filescrubberMCP.Tests.Services;

public class UriServiceTests
{
    private readonly Mock<ILogger<UriService>> _mockLogger;
    private readonly Mock<IAppConfigurationProvider> _mockConfigProvider;
    private readonly UriService _uriService;
    private readonly HttpClient _httpClient;

    public UriServiceTests()
    {
        _mockLogger = new Mock<ILogger<UriService>>();
        _mockConfigProvider = new Mock<IAppConfigurationProvider>();
        _httpClient = new HttpClient();
        _uriService = new UriService(_mockLogger.Object, _mockConfigProvider.Object, _httpClient);
    }

    [Fact]
    public async Task GetAsync_WithInvalidUri_ThrowsException()
    {
        // Arrange
        var invalidUri = "not-a-valid-uri";

        // Act & Assert
        await Assert.ThrowsAsync<System.InvalidOperationException>(() => _uriService.GetAsync(invalidUri));
    }

    // Note: Integration tests that make real HTTP requests should be in a separate test class
    // marked with [Trait("Category", "Integration")] and run separately from unit tests
}
