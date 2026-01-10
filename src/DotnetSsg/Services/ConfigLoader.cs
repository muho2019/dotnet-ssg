using System.Text.Json;
using DotnetSsg.Models;
using Microsoft.Extensions.Logging;

namespace DotnetSsg.Services;

public class ConfigLoader : IConfigLoader
{
    private const string DefaultConfigPath = "config.json";
    private readonly ILogger<ConfigLoader> _logger;

    public ConfigLoader(ILogger<ConfigLoader> logger)
    {
        _logger = logger;
    }

    public async Task<SiteConfig> LoadConfigAsync(string? configPath = null)
    {
        configPath ??= DefaultConfigPath;

        if (!File.Exists(configPath))
        {
            _logger.LogWarning("Warning: Config file '{ConfigPath}' not found. Using default configuration.", configPath);
            return new SiteConfig();
        }

        try
        {
            await using var stream = File.OpenRead(configPath);
            var config = await JsonSerializer.DeserializeAsync<SiteConfig>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (config != null)
            {
                config.BaseUrl = config.BaseUrl?.Trim().TrimEnd('/') ?? string.Empty;
            }

            return config ?? new SiteConfig();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error: Invalid JSON format in '{ConfigPath}'.", configPath);
            throw; // Rethrow to stop the build process
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error: Failed to read config file '{ConfigPath}'.", configPath);
            throw; // Rethrow to stop the build process
        }
    }
}
