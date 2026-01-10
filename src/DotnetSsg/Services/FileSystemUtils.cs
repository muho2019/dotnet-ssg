using Microsoft.Extensions.Logging;

namespace DotnetSsg.Services;

public interface IFileSystemUtils
{
    void DeleteDirectorySafe(string path);
}

public class FileSystemUtils : IFileSystemUtils
{
    private readonly ILogger<FileSystemUtils> _logger;

    public FileSystemUtils(ILogger<FileSystemUtils> logger)
    {
        _logger = logger;
    }

    public void DeleteDirectorySafe(string path)
    {
        const int maxRetries = 3;
        const int delayMs = 100;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                if (!Directory.Exists(path)) return;

                // 먼저 모든 파일을 삭제
                foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        // 개별 파일 삭제 실패는 로그만 남기고 무시 (서버가 사용 중일 수 있음)
                        _logger.LogWarning("⚠️ 파일 삭제 실패: {FilePath} - {Message}", file, ex.Message);
                    }
                }

                // 빈 디렉토리 삭제 시도
                foreach (var dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Reverse())
                {
                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(dir).Any())
                        {
                            Directory.Delete(dir, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("⚠️ 디렉토리 삭제 실패: {DirPath} - {Message}", dir, ex.Message);
                    }
                }
                
                // 최상위 디렉토리 삭제 시도
                if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any())
                {
                    Directory.Delete(path, false);
                }

                return; // 성공
            }
            catch (Exception ex)
            {
                if (i == maxRetries - 1)
                {
                    _logger.LogWarning("⚠️ 일부 파일 삭제 실패 (진행 중): {Message}", ex.Message);
                    return;
                }

                Thread.Sleep(delayMs);
            }
        }
    }
}
