using DotnetSsg.Models;
using Microsoft.Extensions.Logging;

namespace DotnetSsg.Services;

public class CssBuilder : ICssBuilder
{
    private readonly ILogger<CssBuilder> _logger;

    public CssBuilder(ILogger<CssBuilder> logger)
    {
        _logger = logger;
    }

    public async Task BuildTailwindCssAsync(string workingDirectory)
    {
        try
        {
            _logger.LogInformation("🎨 Tailwind CSS 빌드 중...");

            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = isWindows ? "powershell.exe" : "npm",
                Arguments = isWindows ? "-NoProfile -Command \"npm run css:build\"" : "run css:build",
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process == null)
            {
                _logger.LogWarning("⚠️ npm을 실행할 수 없습니다. Tailwind CSS 빌드를 건너뜁니다.");
                _logger.LogWarning("   npm이 설치되어 있고 PATH에 등록되어 있는지 확인하세요.");
                return;
            }

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("✅ Tailwind CSS 빌드 완료");
            }
            else
            {
                var error = await process.StandardError.ReadToEndAsync();
                _logger.LogWarning("⚠️ Tailwind CSS 빌드 실패: {Error}", error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Tailwind CSS 빌드 중 오류: {Message}", ex.Message);
        }
    }
}
