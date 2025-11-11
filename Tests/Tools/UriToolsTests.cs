using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Tools;
using filescrubberMCP.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace filescrubberMCP.Tests.Tools;

public class UriToolsTests
{
    private readonly Mock<ILogger<UriTools>> _mockLogger;
    private readonly Mock<IUriService> _mockUriService;
    private readonly UriTools _uriTools;

    public UriToolsTests()
    {
        _mockLogger = new Mock<ILogger<UriTools>>();
        _mockUriService = new Mock<IUriService>();
        _uriTools = new UriTools(_mockLogger.Object, _mockUriService.Object);
    }

    [Fact]
    public async Task Get_WithValidUri_ReturnsSuccess()
    {
        // Arrange
        var uri = "https://example.com";
        var expectedContent = "{\"data\":\"test\"}";
        _mockUriService.Setup(x => x.GetAsync(uri, It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedContent);

        // Act
        var result = await _uriTools.Get(uri);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains("data", result);
        Assert.Contains("test", result);
        _mockUriService.Verify(x => x.GetAsync(uri, It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task Get_WithHeaders_PassesHeaders()
    {
        // Arrange
        var uri = "https://example.com";
        var headersJson = "{\"Authorization\":\"Bearer token\"}";
        _mockUriService.Setup(x => x.GetAsync(uri, It.Is<Dictionary<string, string>>(h => h.ContainsKey("Authorization"))))
            .ReturnsAsync("response");

        // Act
        var result = await _uriTools.Get(uri, headersJson);

        // Assert
        Assert.Contains("\"success\": true", result);
        _mockUriService.Verify(x => x.GetAsync(uri, It.Is<Dictionary<string, string>>(h => h.ContainsKey("Authorization"))), Times.Once);
    }

    [Fact]
    public async Task Post_WithJsonBody_ReturnsSuccess()
    {
        // Arrange
        var uri = "https://example.com/api";
        var jsonBody = "{\"key\":\"value\"}";
        _mockUriService.Setup(x => x.PostAsync(uri, jsonBody, It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync("response");

        // Act
        var result = await _uriTools.Post(uri, jsonBody);

        // Assert
        Assert.Contains("\"success\": true", result);
        _mockUriService.Verify(x => x.PostAsync(uri, jsonBody, It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task Put_WithJsonBody_ReturnsSuccess()
    {
        // Arrange
        var uri = "https://example.com/api/1";
        var jsonBody = "{\"updated\":\"value\"}";
        _mockUriService.Setup(x => x.PutAsync(uri, jsonBody, It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync("response");

        // Act
        var result = await _uriTools.Put(uri, jsonBody);

        // Assert
        Assert.Contains("\"success\": true", result);
        _mockUriService.Verify(x => x.PutAsync(uri, jsonBody, It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task Patch_WithJsonBody_ReturnsSuccess()
    {
        // Arrange
        var uri = "https://example.com/api/1";
        var jsonBody = "{\"field\":\"value\"}";
        _mockUriService.Setup(x => x.PatchAsync(uri, jsonBody, It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync("response");

        // Act
        var result = await _uriTools.Patch(uri, jsonBody);

        // Assert
        Assert.Contains("\"success\": true", result);
        _mockUriService.Verify(x => x.PatchAsync(uri, jsonBody, It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithValidUri_ReturnsSuccess()
    {
        // Arrange
        var uri = "https://example.com/api/1";
        _mockUriService.Setup(x => x.DeleteAsync(uri, It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync("deleted");

        // Act
        var result = await _uriTools.Delete(uri);

        // Assert
        Assert.Contains("\"success\": true", result);
        _mockUriService.Verify(x => x.DeleteAsync(uri, It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task Head_WithValidUri_ReturnsSuccess()
    {
        // Arrange
        var uri = "https://example.com";
        var headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };
        _mockUriService.Setup(x => x.HeadAsync(uri, It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(headers);

        // Act
        var result = await _uriTools.Head(uri);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains("headers", result);
        _mockUriService.Verify(x => x.HeadAsync(uri, It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task Options_WithValidUri_ReturnsSuccess()
    {
        // Arrange
        var uri = "https://example.com";
        _mockUriService.Setup(x => x.OptionsAsync(uri, It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync("OPTIONS response");

        // Act
        var result = await _uriTools.Options(uri);

        // Assert
        Assert.Contains("\"success\": true", result);
        _mockUriService.Verify(x => x.OptionsAsync(uri, It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task Get_WhenServiceThrowsException_ReturnsError()
    {
        // Arrange
        var uri = "https://example.com";
        _mockUriService.Setup(x => x.GetAsync(uri, It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new System.Net.Http.HttpRequestException("Network error"));

        // Act
        var result = await _uriTools.Get(uri);

        // Assert
        Assert.Contains("\"success\": false", result);
        Assert.Contains("error", result);
        Assert.Contains("Network error", result);
    }
}
