namespace DotnetSsg.Services;

public class FileScanner : IFileScanner
{
    public IEnumerable<string> Scan(string directory, string extension)
    {
        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"경고: 디렉토리 '{directory}'을(를) 찾을 수 없습니다.");
            return Enumerable.Empty<string>();
        }

        return Directory.EnumerateFiles(directory, $"*.{extension}", SearchOption.AllDirectories);
    }
}
