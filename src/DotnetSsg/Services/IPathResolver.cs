using DotnetSsg.Models;

namespace DotnetSsg.Services;

public interface IPathResolver
{
    /// <summary>
    /// 웹에서 접근 가능한 URL을 반환합니다 (예: /posts/hello-world)
    /// </summary>
    string GetUrl(ContentItem item);

    /// <summary>
    /// 실제 HTML 파일이 생성될 디스크 상의 출력 경로를 반환합니다.
    /// </summary>
    string GetOutputPath(ContentItem item);
}
