using System.CommandLine;
using System.Text;
using DotnetSsg.Commands;
using DotnetSsg.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// 콘솔 UTF-8 인코딩 설정 (이모지 출력을 위해)
Console.OutputEncoding = Encoding.UTF8;

// 1. 서비스 등록
var serviceCollection = new ServiceCollection();

// 로깅 설정
serviceCollection.AddLogging(builder =>
{
    builder.AddConsole();
    // 기본 로깅 노이즈를 줄이되 앱 로그는 보이도록 설정
    // 기본적으로 Information으로 설정하거나, Warning으로 유지하되 중요한 빌드 단계에는 Information을 사용
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddFilter("Microsoft", LogLevel.Warning);
    builder.AddFilter("System", LogLevel.Warning);
});

// Singleton: 상태가 없거나 전역적으로 공유되는 서비스
serviceCollection.AddSingleton<IConfigLoader, ConfigLoader>();
serviceCollection.AddSingleton<IFileScanner, FileScanner>();
serviceCollection.AddSingleton<IStaticFileCopier, StaticFileCopier>();
serviceCollection.AddSingleton<ICssBuilder, CssBuilder>();
serviceCollection.AddSingleton<ISitemapGenerator, SitemapGenerator>();
serviceCollection.AddSingleton<IRobotsTxtGenerator, RobotsTxtGenerator>();
serviceCollection.AddSingleton<IRssFeedGenerator, RssFeedGenerator>();
serviceCollection.AddSingleton<IFileSystemUtils, FileSystemUtils>();
serviceCollection.AddSingleton<IPathResolver, PathResolver>();
serviceCollection.AddSingleton<ISeoService, SeoService>();

// Scoped: 빌드/요청 단위로 상태가 관리되어야 하는 서비스
// (BlazorRenderer, HtmlGenerator 등은 내부적으로 상태를 가질 수 있음)
serviceCollection.AddScoped<IBlazorRenderer, BlazorRenderer>();
serviceCollection.AddScoped<IMarkdownParser, MarkdownParser>();
serviceCollection.AddScoped<IHtmlGenerator, HtmlGenerator>();

// Render Strategies
serviceCollection.AddScoped<DotnetSsg.Services.RenderStrategies.IRenderStrategy, DotnetSsg.Services.RenderStrategies.PostRenderStrategy>();
serviceCollection.AddScoped<DotnetSsg.Services.RenderStrategies.IRenderStrategy, DotnetSsg.Services.RenderStrategies.PageRenderStrategy>();


// Transient: 가볍고 필요할 때마다 생성해도 되는 서비스
serviceCollection.AddTransient<IBuildService, BuildService>();

var serviceProvider = serviceCollection.BuildServiceProvider();

// 2. 명령어 구성 및 실행
var rootCommand = new RootCommand("dotnet-ssg - .NET 기반 정적 사이트 생성기");

// 서비스 주입을 통해 명령어 생성
rootCommand.Add(BuildCommand.Create(
    serviceProvider.GetRequiredService<IBuildService>(),
    serviceProvider.GetRequiredService<ICssBuilder>()));

rootCommand.Add(ServeCommand.Create(
    serviceProvider.GetRequiredService<IBuildService>(),
    serviceProvider.GetRequiredService<ICssBuilder>()));

rootCommand.Add(CleanCommand.Create());
rootCommand.Add(NewCommand.Create());
rootCommand.Add(InitCommand.Create());

// 인자 없이 실행하면 도움말 표시
if (args.Length == 0)
{
    args = ["--help"];
}

var parseResult = rootCommand.Parse(args);

return await parseResult.InvokeAsync();