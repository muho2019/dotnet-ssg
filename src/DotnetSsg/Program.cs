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
    var robotsTxtGenerator = new RobotsTxtGenerator();
    var rssFeedGenerator = new RssFeedGenerator();

    // 2. 설정 로드
    Console.WriteLine("설정 로딩 중...");
    var siteConfig = await configLoader.LoadConfigAsync(configPath);

    // 3. 정적 파일 복사
    Console.WriteLine("정적 파일 복사 중...");
    staticFileCopier.Copy(staticDir, Path.Combine(outputDir, "static"));

    // Favicon 및 기타 루트 파일 복사 (content 폴더의 파일들을 output 루트로)
    string[] rootFiles = [ "favicon.ico" ];
    foreach (var rootFile in rootFiles)
    {
        var sourcePath = Path.Combine(contentDir, rootFile);
        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, Path.Combine(outputDir, rootFile), true);
        }
    }
    
    // Google Search Console 확인 파일 등 HTML 파일 복사
    var htmlFiles = Directory.GetFiles(contentDir, "*.html");
    foreach (var htmlFile in htmlFiles)
    {
        var fileName = Path.GetFileName(htmlFile);
        File.Copy(htmlFile, Path.Combine(outputDir, fileName), true);
    }

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
            var contentItem = await markdownParser.ParseAsync(file, siteConfig.BaseUrl);
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

    // 인덱스 페이지 (Home)
    var indexTemplatePath = Path.Combine(templatesDir, "index.liquid");
    if (File.Exists(indexTemplatePath))
    {
        var sortedPostsForIndex = posts.OrderByDescending(p => p.Date).ToList();
        var indexHtml = await templateRenderer.RenderAsync(indexTemplatePath, new { site = siteConfig, posts = sortedPostsForIndex });
        await File.WriteAllTextAsync(Path.Combine(outputDir, "index.html"), indexHtml);
    }

    // 태그별 아카이브
    var tags = posts.SelectMany(p => p.Tags ?? Enumerable.Empty<string>()).Distinct();
    var tagTemplatePath = Path.Combine(templatesDir, "tag_archive.liquid");
    if (File.Exists(tagTemplatePath))
    {
        var sortedPostsForTags = posts.OrderByDescending(p => p.Date).ToList();
        foreach (var tag in tags)
        {
            var tagPosts = sortedPostsForTags.Where(p => p.Tags != null && p.Tags.Contains(tag)).ToList();
            var tagHtml = await templateRenderer.RenderAsync(tagTemplatePath, new { site = siteConfig, tag = tag, posts = tagPosts });
            
            var tagDir = Path.Combine(outputDir, "tags", tag);
            Directory.CreateDirectory(tagDir);
            await File.WriteAllTextAsync(Path.Combine(tagDir, "index.html"), tagHtml);
        }
    }

    // 7. 사이트맵을 위한 콘텐츠 항목 준비
    Console.WriteLine("사이트맵 생성 중...");
    var sortedPosts = posts.OrderByDescending(p => p.Date).ToList();
    sitemapGenerator.Generate(siteConfig, contentItems.ToList(), outputDir, sortedPosts, tags.ToList());

    // 8. robots.txt 생성
    Console.WriteLine("robots.txt 생성 중...");
    robotsTxtGenerator.Generate(siteConfig, outputDir);

    // 9. RSS 피드 생성
    Console.WriteLine("RSS 피드 생성 중...");
    rssFeedGenerator.Generate(siteConfig, sortedPosts, outputDir);

    stopwatch.Stop();
    Console.WriteLine($"✅ 빌드가 {stopwatch.ElapsedMilliseconds}ms만에 성공적으로 완료되었습니다.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"❌ 빌드 실패: {ex.Message}");
    Environment.Exit(1);
}