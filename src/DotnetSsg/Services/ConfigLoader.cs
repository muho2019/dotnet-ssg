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
            _logger.LogWarning("경고: 설정 파일 '{ConfigPath}'을(를) 찾을 수 없습니다. 기본 설정을 사용합니다.", configPath);
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
            _logger.LogError(ex, "오류: '{ConfigPath}'의 JSON 형식이 잘못되었습니다.", configPath);
            throw; // 빌드 프로세스를 중단하기 위해 다시 던짐
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "오류: 설정 파일 '{ConfigPath}'을(를) 읽는데 실패했습니다.", configPath);
            throw; // 빌드 프로세스를 중단하기 위해 다시 던짐
        }
    }
}
