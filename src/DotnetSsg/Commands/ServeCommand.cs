using System.CommandLine;
using System.CommandLine.Invocation;
using DotnetSsg.Services;

namespace DotnetSsg.Commands;

public static class ServeCommand
{
    public static Command Create()
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

        command.Action = new AsynchronousServeAction(portOption, outputOption, draftsOption, noWatchOption);

        return command;
    }

    private class AsynchronousServeAction : AsynchronousCommandLineAction
    {
        private readonly Option<int> _portOption;
        private readonly Option<string> _outputOption;
        private readonly Option<bool> _draftsOption;
        private readonly Option<bool> _noWatchOption;

        public AsynchronousServeAction(Option<int> portOption, Option<string> outputOption,
            Option<bool> draftsOption, Option<bool> noWatchOption)
        {
            _portOption = portOption;
            _outputOption = outputOption;
            _draftsOption = draftsOption;
            _noWatchOption = noWatchOption;
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
            var buildService = new BuildService();
            var buildSuccess = await buildService.BuildAsync(workingDirectory, output, drafts);

            if (buildSuccess)
            {
                // Tailwind CSS 빌드
                await BuildTailwindCssAsync(workingDirectory);
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
                fileWatcher.OnChange += async (sender, changedFile) =>
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

                        var rebuildService = new BuildService();
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        var success = await rebuildService.BuildAsync(workingDirectory, output, true); // draft 포함

                        if (success)
                        {
                            // BuildService가 output을 삭제하므로 항상 Tailwind CSS 재빌드 필요
                            await BuildTailwindCssAsync(workingDirectory);

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

        private static async Task BuildTailwindCssAsync(string workingDirectory)
        {
            try
            {
                Console.WriteLine("🎨 Tailwind CSS 빌드 중...");

                var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = isWindows ? "powershell.exe" : "npm",
                    Arguments = isWindows ? "-NoProfile -Command \"npm run css:build\"" : "run css:build",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                if (process == null)
                {
                    Console.WriteLine("⚠️ npm을 실행할 수 없습니다. Tailwind CSS 빌드를 건너뜁니다.");
                    return;
                }

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("✅ Tailwind CSS 빌드 완료");
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    Console.WriteLine($"⚠️ Tailwind CSS 빌드 실패: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Tailwind CSS 빌드 중 오류: {ex.Message}");
            }
        }
    }
}
