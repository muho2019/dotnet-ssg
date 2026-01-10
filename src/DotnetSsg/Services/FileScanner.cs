namespace DotnetSsg.Services;

public class FileScanner : IFileScanner
{
    public IEnumerable<string> Scan(string directory, string extension)
    {
        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"Warning: Directory '{directory}' not found.");
            return Enumerable.Empty<string>();
        }

        return Directory.EnumerateFiles(directory, $"*.{extension}", SearchOption.AllDirectories);
    }
}
