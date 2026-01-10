using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class PathResolver : IPathResolver
{
    private const string OutputDirectory = "output";

    public string GetUrl(ContentItem item, string contentRoot = "content")
    {
        // 이미 URL이 명시적으로 지정된 경우 그대로 사용
        if (!string.IsNullOrWhiteSpace(item.Url))
        {
            return item.Url.StartsWith('/') ? item.Url : $"/{item.Url}";
        }

        var relativePath = Path.GetRelativePath(contentRoot, item.SourcePath);
        var pathWithoutExt = Path.ChangeExtension(relativePath, null);

        // Windows 역슬래시를 웹 표준 슬래시로 변경
        var url = "/" + pathWithoutExt.Replace('\\', '/');

        // index 파일인 경우 디렉토리 루트로 (예: /about/index -> /about/)
        if (url.EndsWith("/index", StringComparison.OrdinalIgnoreCase))
        {
            url = url.Substring(0, url.Length - 5); // "/index" 제거 -> "/about/"이 아니라 "/about"이 됨
            
            // 만약 "/about" 이 되었다면 "/about/" 로 끝나게 할지 결정해야 함.
            // 기존 로직: "/about/index" -> "/about" -> (if empty then empty else add slash)
            
            if (url == "/") return "/"; // 루트 인덱스
            
            // 루트가 아닌 경우 "/about" -> "/about/" 
            // 하지만 위 substring 로직은 url="/index" -> url="/" (길이 6 - 5 = 1) -> OK
            // url="/about/index" -> url="/about/" (길이 12 - 5 = 7) -> OK
            // url="/posts/hello/index" -> url="/posts/hello/" -> OK
        }
        else
        {
             // 일반 파일: /about -> /about/
             url += "/";
        }

        return url;
    }

    public string GetOutputPath(ContentItem item, string contentRoot = "content")
    {
        var relativePath = Path.GetRelativePath(contentRoot, item.SourcePath);
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
