using DotnetSsg.Models;

namespace DotnetSsg.Services;

/// <summary>
/// 파일 시스템 경로와 웹 URL 간의 변환을 담당하는 서비스 인터페이스입니다.
/// </summary>
public interface IPathResolver
{
    /// <summary>
    /// 웹에서 접근 가능한 URL을 반환합니다 (예: /posts/hello-world)
    /// </summary>
    /// <param name="item">콘텐츠 아이템</param>
    /// <param name="contentRoot">콘텐츠 루트 디렉토리 (기본값: content)</param>
    /// <returns>웹 URL</returns>
    string GetUrl(ContentItem item, string contentRoot = "content");

    /// <summary>
    /// 실제 HTML 파일이 생성될 디스크 상의 출력 경로를 반환합니다.
    /// </summary>
    /// <param name="item">콘텐츠 아이템</param>
    /// <param name="contentRoot">콘텐츠 루트 디렉토리 (기본값: content)</param>
    /// <returns>상대적 출력 파일 경로</returns>
    string GetOutputPath(ContentItem item, string contentRoot = "content");
}
