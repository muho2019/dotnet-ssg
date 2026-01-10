using DotnetSsg.Models;

namespace DotnetSsg.Services;

/// <summary>
/// Blazor 컴포넌트를 HTML 문자열로 렌더링하는 서비스 인터페이스입니다.
/// </summary>
public interface IBlazorRenderer : IAsyncDisposable
{
    /// <summary>
    /// 지정된 컴포넌트를 비동기적으로 렌더링합니다.
    /// </summary>
    /// <typeparam name="TComponent">렌더링할 컴포넌트 타입</typeparam>
    /// <param name="parameters">컴포넌트에 전달할 파라미터 딕셔너리</param>
    /// <returns>렌더링된 HTML 문자열</returns>
    Task<string> RenderComponentAsync<TComponent>(Dictionary<string, object?> parameters) where TComponent : Microsoft.AspNetCore.Components.IComponent;
}

/// <summary>
/// 전체 정적 사이트 빌드 프로세스를 조정하는 서비스 인터페이스입니다.
/// </summary>
public interface IBuildService
{
    /// <summary>
    /// 사이트 빌드를 실행합니다.
    /// </summary>
    /// <param name="workingDirectory">작업 디렉토리 경로</param>
    /// <param name="outputPath">출력 디렉토리 경로 (기본값: output)</param>
    /// <param name="includeDrafts">초안(draft) 포함 여부</param>
    /// <returns>빌드 성공 여부</returns>
    Task<bool> BuildAsync(string workingDirectory, string outputPath = "output", bool includeDrafts = false);
}

/// <summary>
/// 사이트 설정 파일을 로드하는 서비스 인터페이스입니다.
/// </summary>
public interface IConfigLoader
{
    /// <summary>
    /// 설정 파일(config.json)을 비동기적으로 로드합니다.
    /// </summary>
    /// <param name="configPath">설정 파일 경로 (선택 사항)</param>
    /// <returns>로드된 사이트 설정 객체</returns>
    Task<SiteConfig> LoadConfigAsync(string? configPath = null);
}

/// <summary>
/// 파일 시스템 스캔을 수행하는 서비스 인터페이스입니다.
/// </summary>
public interface IFileScanner
{
    /// <summary>
    /// 지정된 디렉토리에서 특정 확장자를 가진 파일들을 검색합니다.
    /// </summary>
    /// <param name="directory">검색할 디렉토리 경로</param>
    /// <param name="extension">검색할 파일 확장자 (예: *.md)</param>
    /// <returns>검색된 파일 경로 목록</returns>
    IEnumerable<string> Scan(string directory, string extension);
}

/// <summary>
/// 정적 파일(이미지, CSS 등)을 복사하는 서비스 인터페이스입니다.
/// </summary>
public interface IStaticFileCopier
{
    /// <summary>
    /// 소스 디렉토리의 모든 내용을 대상 디렉토리로 복사합니다.
    /// </summary>
    /// <param name="sourceDirectory">소스 디렉토리 경로</param>
    /// <param name="destinationDirectory">대상 디렉토리 경로</param>
    void Copy(string sourceDirectory, string destinationDirectory);
}

/// <summary>
/// 마크다운 파일을 파싱하여 콘텐츠 객체로 변환하는 서비스 인터페이스입니다.
/// </summary>
public interface IMarkdownParser
{
    /// <summary>
    /// 마크다운 파일을 비동기적으로 파싱합니다.
    /// </summary>
    /// <param name="filePath">마크다운 파일 경로</param>
    /// <param name="contentRoot">콘텐츠 루트 디렉토리 경로</param>
    /// <returns>파싱된 콘텐츠 아이템</returns>
    Task<ContentItem> ParseAsync(string filePath, string contentRoot);
}

/// <summary>
/// 콘텐츠 아이템을 기반으로 HTML 파일을 생성하는 서비스 인터페이스입니다.
/// </summary>
public interface IHtmlGenerator
{
    /// <summary>
    /// 단일 콘텐츠 아이템(페이지 또는 포스트)에 대한 HTML을 생성합니다.
    /// </summary>
    /// <param name="item">콘텐츠 아이템</param>
    /// <param name="siteConfig">사이트 설정</param>
    Task GenerateAsync(ContentItem item, SiteConfig siteConfig);

    /// <summary>
    /// 메인 인덱스 페이지(홈)를 생성합니다.
    /// </summary>
    /// <param name="siteConfig">사이트 설정</param>
    /// <param name="posts">포스트 목록</param>
    /// <param name="outputDirectory">출력 디렉토리</param>
    Task GenerateIndexAsync(SiteConfig siteConfig, List<Post> posts, string outputDirectory);

    /// <summary>
    /// 태그 아카이브 페이지를 생성합니다.
    /// </summary>
    /// <param name="siteConfig">사이트 설정</param>
    /// <param name="tag">태그 이름</param>
    /// <param name="posts">해당 태그를 포함한 포스트 목록</param>
    /// <param name="outputDirectory">출력 디렉토리</param>
    Task GenerateTagArchiveAsync(SiteConfig siteConfig, string tag, List<Post> posts, string outputDirectory);
}

/// <summary>
/// CSS 빌드(Tailwind CSS 등)를 처리하는 서비스 인터페이스입니다.
/// </summary>
public interface ICssBuilder
{
    /// <summary>
    /// Tailwind CSS 빌드를 비동기적으로 실행합니다.
    /// </summary>
    /// <param name="workingDirectory">작업 디렉토리 경로</param>
    Task BuildTailwindCssAsync(string workingDirectory);
}

/// <summary>
/// sitemap.xml을 생성하는 서비스 인터페이스입니다.
/// </summary>
public interface ISitemapGenerator
{
    /// <summary>
    /// 사이트맵을 생성합니다.
    /// </summary>
    /// <param name="config">사이트 설정</param>
    /// <param name="contentItems">모든 콘텐츠 아이템 목록</param>
    /// <param name="outputDirectory">출력 디렉토리</param>
    /// <param name="posts">포스트 목록</param>
    /// <param name="tags">태그 목록</param>
    void Generate(SiteConfig config, List<ContentItem> contentItems, string outputDirectory, List<Post> posts, List<string> tags);
}

/// <summary>
/// robots.txt 파일을 생성하는 서비스 인터페이스입니다.
/// </summary>
public interface IRobotsTxtGenerator
{
    /// <summary>
    /// robots.txt 파일을 생성합니다.
    /// </summary>
    /// <param name="config">사이트 설정</param>
    /// <param name="outputDirectory">출력 디렉토리</param>
    void Generate(SiteConfig config, string outputDirectory);
}

/// <summary>
/// RSS 피드(feed.xml)를 생성하는 서비스 인터페이스입니다.
/// </summary>
public interface IRssFeedGenerator
{
    /// <summary>
    /// RSS 피드를 생성합니다.
    /// </summary>
    /// <param name="config">사이트 설정</param>
    /// <param name="posts">포스트 목록</param>
    /// <param name="outputDirectory">출력 디렉토리</param>
    void Generate(SiteConfig config, List<Post> posts, string outputDirectory);
}
