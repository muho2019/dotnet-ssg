using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetSsg.Services;

public class BlazorRenderer : IAsyncDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private HtmlRenderer? _htmlRenderer;
    private bool _disposed = false;

    public BlazorRenderer()
    {
        var services = new ServiceCollection();

        // 로깅 설정
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        _serviceProvider = services.BuildServiceProvider();
        _loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    /// <summary>
    /// Blazor 컴포넌트를 정적 HTML로 렌더링합니다.
    /// </summary>
    public async Task<string> RenderComponentAsync<TComponent>(Dictionary<string, object?> parameters)
        where TComponent : IComponent
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BlazorRenderer));

        _htmlRenderer ??= new HtmlRenderer(_serviceProvider, _loggerFactory);

        var html = await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameterView = ParameterView.FromDictionary(parameters);
            var output = await _htmlRenderer.RenderComponentAsync<TComponent>(parameterView);
            return output.ToHtmlString();
        });

        return html;
    }

    /// <summary>
    /// 리소스 정리
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            if (_htmlRenderer != null)
            {
                await _htmlRenderer.DisposeAsync();
                _htmlRenderer = null;
            }
        }
        catch (Exception disposeEx)
        {
            // Dispose 에러는 무시하지만, 디버깅을 위해 로그를 남깁니다.
            await Console.Error.WriteLineAsync(
                $"⚠️ HtmlRenderer Dispose 중 예외 발생: {disposeEx.GetType().Name}: {disposeEx.Message}");
            await Console.Error.WriteLineAsync(disposeEx.StackTrace);
        }

        try
        {
            await _serviceProvider.DisposeAsync();
        }
        catch (Exception disposeEx)
        {
            // Dispose 에러는 무시하지만, 디버깅을 위해 로그를 남깁니다.
            await Console.Error.WriteLineAsync(
                $"⚠️ ServiceProvider Dispose 중 예외 발생: {disposeEx.GetType().Name}: {disposeEx.Message}");
            await Console.Error.WriteLineAsync(disposeEx.StackTrace);
        }
    }
}