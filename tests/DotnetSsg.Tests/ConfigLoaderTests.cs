using DotnetSsg.Services;
using DotnetSsg.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace DotnetSsg.Tests.Services;

public class ConfigLoaderTests
{
    private readonly Mock<ILogger<ConfigLoader>> _loggerMock;
    private readonly ConfigLoader _configLoader;
    private readonly string _testConfigPath;

    public ConfigLoaderTests()
    {
        _loggerMock = new Mock<ILogger<ConfigLoader>>();
        _configLoader = new ConfigLoader(_loggerMock.Object);
        _testConfigPath = Path.Combine(Path.GetTempPath(), $"config-{Guid.NewGuid()}.json");
    }

    [Fact]
    public async Task LoadConfigAsync_ShouldReturnDefaultConfig_WhenFileDoesNotExist()
    {
        // Act
        var result = await _configLoader.LoadConfigAsync("non-existent-config.json");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("My Awesome Blog", result.Title); // Default title from SiteConfig constructor
        
        // Verify logger was called with warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadConfigAsync_ShouldLoadConfig_WhenFileExists()
    {
        // Arrange
        var config = new SiteConfig
        {
            Title = "Test Site",
            BaseUrl = "https://example.com"
        };
        var json = JsonSerializer.Serialize(config);
        await File.WriteAllTextAsync(_testConfigPath, json);

        try
        {
            // Act
            var result = await _configLoader.LoadConfigAsync(_testConfigPath);

            // Assert
            Assert.Equal("Test Site", result.Title);
            Assert.Equal("https://example.com", result.BaseUrl);
        }
        finally
        {
            File.Delete(_testConfigPath);
        }
    }

    [Fact]
    public async Task LoadConfigAsync_ShouldTrimBaseUrl()
    {
        // Arrange
        var config = new SiteConfig
        {
            BaseUrl = "https://example.com/blog/"
        };
        var json = JsonSerializer.Serialize(config);
        await File.WriteAllTextAsync(_testConfigPath, json);

        try
        {
            // Act
            var result = await _configLoader.LoadConfigAsync(_testConfigPath);

            // Assert
            Assert.Equal("https://example.com/blog", result.BaseUrl); // Trailing slash removed
        }
        finally
        {
            File.Delete(_testConfigPath);
        }
    }

    [Fact]
    public async Task LoadConfigAsync_ShouldThrow_WhenJsonIsInvalid()
    {
        // Arrange
        await File.WriteAllTextAsync(_testConfigPath, "{ invalid json }");

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => _configLoader.LoadConfigAsync(_testConfigPath));
            
            // Verify logger was called with error
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid JSON")),
                    It.IsAny<JsonException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            File.Delete(_testConfigPath);
        }
    }
}
