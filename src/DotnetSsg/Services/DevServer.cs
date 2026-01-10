using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace DotnetSsg.Services;

public class DevServer
{
    private readonly string _contentRoot;
    private readonly int _port;
    private readonly List<WebSocket> _websockets = new();
    private readonly object _websocketsLock = new();

    public DevServer(string contentRoot, int port)
    {
        _contentRoot = contentRoot;
        _port = port;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            WebRootPath = _contentRoot,
            ContentRootPath = _contentRoot
        });

        // Kestrel 설정
        builder.WebHost.ConfigureKestrel(options => { options.Listen(IPAddress.Any, _port); });

        // 로깅 최소화 (개발 서버 출력을 깔끔하게)
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        var app = builder.Build();

        // WebSocket 지원
        app.UseWebSockets();

        // LiveReload WebSocket 엔드포인트 미들웨어
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/livereload")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    lock (_websocketsLock)
                    {
                        _websockets.Add(webSocket);
                        Console.WriteLine($"[DevServer] WebSocket 연결됨. 총 연결: {_websockets.Count}");
                    }

                    try
                    {
                        // 연결 유지 (클라이언트로부터 메시지 대기)
                        var buffer = new byte[1024];
                        while (webSocket.State == WebSocketState.Open)
                        {
                            var result = await webSocket.ReceiveAsync(
                                new ArraySegment<byte>(buffer),
                                cancellationToken);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await webSocket.CloseAsync(
                                    WebSocketCloseStatus.NormalClosure,
                                    "",
                                    cancellationToken);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DevServer] WebSocket 오류: {ex.Message}");
                    }
                    finally
                    {
                        lock (_websocketsLock)
                        {
                            _websockets.Remove(webSocket);
                            Console.WriteLine($"[DevServer] WebSocket 연결 해제. 남은 연결: {_websockets.Count}");
                        }
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("WebSocket 요청이 아닙니다.");
                }

                return; // 처리 완료
            }

            await next(context);
        });

        // HTML 파일에 LiveReload 스크립트 주입하는 미들웨어 (정적 파일보다 먼저 실행)
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value ?? "/";

            // 디렉토리 경로는 index.html로 변환
            if (path.EndsWith("/"))
            {
                path += "index.html";
            }
            else if (!Path.HasExtension(path))
            {
                path += "/index.html";
            }

            var filePath = Path.Combine(_contentRoot, path.TrimStart('/'));

            // HTML 파일인 경우 스크립트 주입 (단, 너무 큰 파일은 제외)
            const long maxHtmlInjectionSizeBytes = 5L * 1024 * 1024; // 5 MB
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists && fileInfo.Extension.Equals(".html", StringComparison.OrdinalIgnoreCase))
            {
                if (fileInfo.Length <= maxHtmlInjectionSizeBytes)
                {
                    var html = await File.ReadAllTextAsync(filePath, cancellationToken);

                    // LiveReload 스크립트 주입
                    html = InjectLiveReloadScript(html);

                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(html, cancellationToken);
                    return; // 처리 완료, 다음 미들웨어로 가지 않음
                }
                // 파일이 너무 큰 경우에는 LiveReload 스크립트를 주입하지 않고
                // 다음 미들웨어(예: 정적 파일 미들웨어)에 처리를 위임함
            }

            // HTML이 아닌 경우 또는 너무 큰 HTML 파일인 경우 다음 미들웨어로 전달
            await next(context);
        });

        // HTML 파일은 정적 파일 미들웨어로 서빙하지 않음
        app.Use(async (HttpContext context, Func<Task> next) =>
        {
            var path = context.Request.Path;
            if (path.HasValue && path.Value.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await next();
        });

        // 정적 파일 제공 (HTML 제외)
        var fileProvider = new PhysicalFileProvider(_contentRoot);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider,
            RequestPath = "",
            ServeUnknownFileTypes = false
        });

        // 404 처리
        app.Use(async (HttpContext context, Func<Task> _) =>
        {
            if (context.Response.StatusCode == 404 || !context.Response.HasStarted)
            {
                var notFoundPath = Path.Combine(_contentRoot, "404.html");
                if (File.Exists(notFoundPath))
                {
                    context.Response.StatusCode = 404;
                    var html = await File.ReadAllTextAsync(notFoundPath, cancellationToken);
                    html = InjectLiveReloadScript(html);
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(html, cancellationToken);
                }
                else
                {
                    context.Response.StatusCode = 404;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("404 - Page Not Found", cancellationToken);
                }
            }
        });

        await app.RunAsync(cancellationToken);
    }

    public void TriggerReload()
    {
        lock (_websocketsLock)
        {
            Console.WriteLine($"[DevServer] TriggerReload 호출됨. WebSocket 연결 수: {_websockets.Count}");

            foreach (var ws in _websockets.ToList().Where(ws => ws.State == WebSocketState.Open))
            {
                try
                {
                    var message = Encoding.UTF8.GetBytes("reload");
                    ws.SendAsync(
                        new ArraySegment<byte>(message),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None).GetAwaiter().GetResult();
                    Console.WriteLine("[DevServer] Reload 메시지 전송 성공");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DevServer] WebSocket 전송 실패: {ex.Message}");
                    _websockets.Remove(ws);
                    Console.WriteLine("[DevServer] 전송 실패로 WebSocket 연결을 목록에서 제거했습니다.");
                }
            }
        }
    }

    private string InjectLiveReloadScript(string html)
    {
        // 개발 서버용으로 base href를 루트로 변경 (GitHub Pages 경로 제거)
        var baseTagPattern = new System.Text.RegularExpressions.Regex(
            @"<base\s+href=""[^""]*""\s*/>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        html = baseTagPattern.Replace(html, @"<base href=""/"" />");

        const string liveReloadScript = @"
<script>
(function() {
    let ws;
    let reconnectInterval = 1000;
    
    function connect() {
        ws = new WebSocket('ws://' + window.location.host + '/livereload');
        
        ws.onopen = function() {
            console.log('[LiveReload] Connected');
            reconnectInterval = 1000;
        };
        
        ws.onmessage = function(event) {
            if (event.data === 'reload') {
                console.log('[LiveReload] Reloading page...');
                window.location.reload();
            }
        };
        
        ws.onclose = function() {
            console.log('[LiveReload] Disconnected, reconnecting...');
            setTimeout(connect, reconnectInterval);
            reconnectInterval = Math.min(reconnectInterval * 1.5, 10000);
        };
        
        ws.onerror = function() {
            ws.close();
        };
    }
    
    connect();
})();
</script>";

        // </body> 태그 직전에 스크립트 주입
        var bodyCloseIndex = html.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (bodyCloseIndex != -1)
        {
            html = html.Insert(bodyCloseIndex, liveReloadScript);
        }
        else
        {
            // </body> 태그가 없으면 끝에 추가
            html += liveReloadScript;
        }

        return html;
    }
}
