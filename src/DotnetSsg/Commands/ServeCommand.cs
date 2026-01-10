using System.CommandLine;
using System.CommandLine.Invocation;
using DotnetSsg.Services;

namespace DotnetSsg.Commands;

public static class ServeCommand
{
    public static Command Create(IBuildService buildService, ICssBuilder cssBuilder)
    {
        var portOption = new Option<int>("--port", "-p")
        {
            Description = "개발 서버 포트 번호",
            DefaultValueFactory = _ => 5000
        };

        var outputOption = new Option<string>("--output", "-o")
        {
            Description = "서빙할 출력 디렉토리 경로",
            DefaultValueFactory = _ => "output"
        };

        var draftsOption = new Option<bool>("--drafts", "-d")
        {
            Description = "Draft 포스트 포함 (개발 서버 기본값: true)",
            DefaultValueFactory = _ => true // 개발 서버는 기본적으로 draft 포함
        };

        var noWatchOption = new Option<bool>("--no-watch")
        {
            Description = "파일 변경 감시 비활성화",
            DefaultValueFactory = _ => false
        };

        var command = new Command("serve", "개발 서버를 시작합니다 (Hot Reload 지원)")
        {
            portOption,
            outputOption,
            draftsOption,
            noWatchOption
        };

        command.Action = new AsynchronousServeAction(portOption, outputOption, draftsOption, noWatchOption, buildService, cssBuilder);

        return command;
    }

    private class AsynchronousServeAction : AsynchronousCommandLineAction
    {
        private readonly Option<int> _portOption;
        private readonly Option<string> _outputOption;
        private readonly Option<bool> _draftsOption;
        private readonly Option<bool> _noWatchOption;
        private readonly IBuildService _buildService;
        private readonly ICssBuilder _cssBuilder;

        public AsynchronousServeAction(
            Option<int> portOption,
            Option<string> outputOption,
            Option<bool> draftsOption,
            Option<bool> noWatchOption,
            IBuildService buildService,
            ICssBuilder cssBuilder)
        {
            _portOption = portOption;
            _outputOption = outputOption;
            _draftsOption = draftsOption;
            _noWatchOption = noWatchOption;
            _buildService = buildService;
            _cssBuilder = cssBuilder;
        }

        public override async Task<int> InvokeAsync(ParseResult parseResult,
            CancellationToken cancellationToken = default)
        {
            var port = parseResult.GetValue(_portOption);
            var output = parseResult.GetValue(_outputOption)!;
            var drafts = parseResult.GetValue(_draftsOption);
            var noWatch = parseResult.GetValue(_noWatchOption);

            var workingDirectory = Directory.GetCurrentDirectory();
            var outputPath = Path.Combine(workingDirectory, output);

            // 서버 시작 전 항상 최신 상태로 빌드
            var draftMessage = drafts ? " (draft 포함)" : "";
            Console.WriteLine($"📦 최신 상태로 빌드 중{draftMessage}...\n");

            var buildSuccess = await _buildService.BuildAsync(workingDirectory, output, drafts);

            if (buildSuccess)
            {
                // Tailwind CSS 빌드
                await _cssBuilder.BuildTailwindCssAsync(workingDirectory);
            }
            else
            {
                Console.WriteLine("\n❌ 빌드 실패. 서버를 시작할 수 없습니다.");
                return 1;
            }

            Console.WriteLine();

            Console.WriteLine("🚀 dotnet-ssg 개발 서버 시작");
            Console.WriteLine($"📁 Serving: ./{output}");
            Console.WriteLine($"🌐 Local:   http://localhost:{port}");

            // 네트워크 주소 표시
            try
            {
                var hostName = System.Net.Dns.GetHostName();
                var hostEntry = System.Net.Dns.GetHostEntry(hostName);
                var localIp = hostEntry.AddressList
                    .FirstOrDefault(addr => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                if (localIp != null)
                {
                    Console.WriteLine($"🌐 Network: http://{localIp}:{port}");
                }
            }
            catch
            {
                // 네트워크 주소를 가져올 수 없어도 무시
            }

            if (!noWatch)
            {
                Console.WriteLine("👀 파일 변경 감시 중...");
            }

            Console.WriteLine("\nCtrl+C를 눌러 서버를 종료하세요.\n");

            // DevServer 시작
            var devServer = new DevServer(outputPath, port);

            FileWatcher? fileWatcher = null;
            var isBuilding = false; // 빌드 중복 방지 플래그
            var buildLock = new object();

            if (!noWatch)
            {
                // FileWatcher 시작
                fileWatcher = new FileWatcher(workingDirectory, output);
                fileWatcher.OnChange += async (_, changedFile) =>
                {
                    // 이미 빌드 중이면 무시
                    lock (buildLock)
                    {
                        if (isBuilding)
                        {
                            Console.WriteLine($"⏭️  빌드 진행 중... {changedFile} 변경은 다음 빌드에 반영됩니다.");
                            return;
                        }

                        isBuilding = true;
                    }

                    try
                    {
                        var timestamp = DateTime.Now.ToString("HH:mm:ss");
                        Console.WriteLine($"[{timestamp}] 📝 {changedFile} 변경 감지");
                        Console.WriteLine($"[{timestamp}] ⚙️  재빌드 시작...");

                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        // 여기서는 재빌드 시 BuildService를 직접 생성하지 않고, 
                        // 이미 주입받은 _buildService는 Scoped가 아니므로 직접 사용하기 보다는
                        // 원래는 IServiceScopeFactory를 주입받아 매번 새로운 BuildService를 생성하는게 맞을 수도 있습니다.
                        // 하지만 현재 Program.cs에서 BuildService는 Transient로 등록되어 있고,
                        // ServeCommand 생성 시점에 한 번 주입된 인스턴스를 계속 사용하게 됩니다.
                        // BuildService 내부에서 CreateAsyncScope를 사용하므로 
                        // BuildAsync 메서드는 상태를 공유하지 않고 안전하게 실행될 수 있습니다.
                        var success = await _buildService.BuildAsync(workingDirectory, output, drafts);

                        if (success)
                        {
                            // BuildService가 output을 삭제하므로 항상 Tailwind CSS 재빌드 필요
                            await _cssBuilder.BuildTailwindCssAsync(workingDirectory);

                            stopwatch.Stop();
                            Console.WriteLine($"[{timestamp}] ✅ 재빌드 완료 ({stopwatch.ElapsedMilliseconds}ms)");

                            // LiveReload 트리거
                            devServer.TriggerReload();
                            Console.WriteLine($"[{timestamp}] 🔄 브라우저 새로고침\n");
                        }
                        else
                        {
                            Console.WriteLine($"[{timestamp}] ❌ 재빌드 실패\n");
                        }
                    }
                    finally
                    {
                        lock (buildLock)
                        {
                            isBuilding = false;
                        }
                    }
                };
                fileWatcher.Start();
            }

            try
            {
                await devServer.StartAsync(cancellationToken);
            }
            finally
            {
                fileWatcher?.Dispose();
            }

            return 0;
        }
    }
}
