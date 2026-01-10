using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class PathResolver : IPathResolver
{
    private const string OutputDirectory = "output";

    public string GetUrl(ContentItem item)
    {
        // 이미 URL이 명시적으로 지정된 경우 그대로 사용
        if (!string.IsNullOrWhiteSpace(item.Url))
        {
            return item.Url.StartsWith('/') ? item.Url : $"/{item.Url}";
        }

        var relativePath = Path.GetRelativePath("content", item.SourcePath);
        var pathWithoutExt = Path.ChangeExtension(relativePath, null);

        // Windows 역슬래시를 웹 표준 슬래시로 변경
        var url = "/" + pathWithoutExt.Replace('\\', '/');

        // index 파일인 경우 디렉토리 루트로 (예: /about/index -> /about/)
        if (url.EndsWith("/index", StringComparison.OrdinalIgnoreCase))
        {
            url = url.Substring(0, url.Length - 5);
        }

        return url;
    }

    public string GetOutputPath(ContentItem item)
    {
        var relativePath = Path.GetRelativePath("content", item.SourcePath);
        var pathWithoutExt = Path.ChangeExtension(relativePath, null);
        var fileName = Path.GetFileNameWithoutExtension(item.SourcePath);

        string outputPath;
        if (fileName.Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            // content/about/index.md -> output/about/index.html
            var parentDir = Path.GetDirectoryName(pathWithoutExt);
            outputPath = Path.Combine(OutputDirectory, parentDir ?? string.Empty, "index.html");
        }
        else
        {
            // content/posts/hello.md -> output/posts/hello/index.html (Clean URL)
            outputPath = Path.Combine(OutputDirectory, pathWithoutExt, "index.html");
        }

        return outputPath;
    }
}
