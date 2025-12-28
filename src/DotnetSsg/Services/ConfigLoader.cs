using System.Text.Json;
using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class ConfigLoader
{
    private const string DefaultConfigPath = "config.json";

    public async Task<SiteConfig> LoadConfigAsync(string? configPath = null)
    {
        configPath ??= DefaultConfigPath;

        if (!File.Exists(configPath))
        {
            Console.WriteLine($"Warning: Config file '{configPath}' not found. Using default configuration.");
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
            Console.Error.WriteLine($"Error: Invalid JSON format in '{configPath}'. Details: {ex.Message}");
            throw; // Rethrow to stop the build process
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: Failed to read config file '{configPath}'. Details: {ex.Message}");
            throw; // Rethrow to stop the build process
        }
    }
}
