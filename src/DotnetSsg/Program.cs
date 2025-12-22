using System.Collections.Concurrent;
using System.Diagnostics;
using DotnetSsg.Models;
using DotnetSsg.Services;

var stopwatch = Stopwatch.StartNew();
Console.WriteLine("🚀 dotnet-ssg 빌드를 시작합니다...");

try
{
    // 0. 경로 설정
    var currentDir = Directory.GetCurrentDirectory();
    var contentDir = Path.Combine(currentDir, "content");
    var outputDir = Path.Combine(currentDir, "output");
    var staticDir = Path.Combine(contentDir, "static");
    var templatesDir = Path.Combine(currentDir, "templates");
    var configPath = Path.Combine(currentDir, "config.json");

    // 출력 디렉토리 준비
    if (!Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
    }

    // 1. 서비스 초기화
    var configLoader = new ConfigLoader();
    var fileScanner = new FileScanner();
    var staticFileCopier = new StaticFileCopier();
    var markdownParser = new MarkdownParser();
    var templateRenderer = new TemplateRenderer();
    var htmlGenerator = new HtmlGenerator(templateRenderer);
    var sitemapGenerator = new SitemapGenerator();

    // 2. 설정 로드
    Console.WriteLine("설정 로딩 중...");
    var siteConfig = await configLoader.LoadConfigAsync(configPath);

    // 3. 정적 파일 복사
    Console.WriteLine("정적 파일 복사 중...");
    staticFileCopier.Copy(staticDir, Path.Combine(outputDir, "static"));

    // 4. 콘텐츠 스캔
    Console.WriteLine("콘텐츠 스캔 중...");
    var files = fileScanner.Scan(contentDir, "md");
    Console.WriteLine($"파일 {files.Count()}개를 찾았습니다.");

    // 5. 콘텐츠 파싱 및 HTML 생성 (병렬 처리)
    Console.WriteLine("콘텐츠 파싱 및 생성 중...");
    var contentItems = new ConcurrentBag<ContentItem>();
    
    var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
    await Parallel.ForEachAsync(files, parallelOptions, async (file, ct) =>
    {
        try
        {
            var contentItem = await markdownParser.ParseAsync(file, siteConfig.RepositoryName);
            contentItems.Add(contentItem);
            
            await htmlGenerator.GenerateAsync(contentItem, siteConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"'{file}' 처리 중 오류 발생: {ex.Message}");
        }
    });

    // 6. 인덱스 페이지 및 아카이브 생성
    Console.WriteLine("인덱스 및 아카이브 생성 중...");
    var posts = contentItems.OfType<Post>().ToList();
    var sortedPosts = posts.OrderByDescending(p => p.Date).ToList();

    // 인덱스 페이지 (Home)
    var indexTemplatePath = Path.Combine(templatesDir, "index.liquid");
    if (File.Exists(indexTemplatePath))
    {
        var indexHtml = await templateRenderer.RenderAsync(indexTemplatePath, new { site = siteConfig, posts = sortedPosts });
        await File.WriteAllTextAsync(Path.Combine(outputDir, "index.html"), indexHtml);
    }

    // 태그별 아카이브
    var tags = sortedPosts.SelectMany(p => p.Tags ?? Enumerable.Empty<string>()).Distinct();
    var tagTemplatePath = Path.Combine(templatesDir, "tag_archive.liquid");
    if (File.Exists(tagTemplatePath))
    {
        foreach (var tag in tags)
        {
            var tagPosts = sortedPosts.Where(p => p.Tags != null && p.Tags.Contains(tag)).ToList();
            var tagHtml = await templateRenderer.RenderAsync(tagTemplatePath, new { site = siteConfig, tag = tag, posts = tagPosts });
            
            var tagDir = Path.Combine(outputDir, "tags", tag);
            Directory.CreateDirectory(tagDir);
            await File.WriteAllTextAsync(Path.Combine(tagDir, "index.html"), tagHtml);
        }
    }

    // 7. 사이트맵 생성
    Console.WriteLine("사이트맵 생성 중...");
    sitemapGenerator.Generate(siteConfig, contentItems.ToList(), outputDir);

    stopwatch.Stop();
    Console.WriteLine($"✅ 빌드가 {stopwatch.ElapsedMilliseconds}ms만에 성공적으로 완료되었습니다.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"❌ 빌드 실패: {ex.Message}");
    Environment.Exit(1);
}