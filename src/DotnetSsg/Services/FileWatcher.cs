namespace DotnetSsg.Services;

public class FileWatcher : IDisposable
{
    private readonly string _workingDirectory;
    private readonly string _outputDirectory;
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly Dictionary<string, DateTime> _lastChangeTime = new();
    private readonly object _lock = new();
    private const int DebounceMilliseconds = 300; // 빠른 반응을 위해 300ms

    public event EventHandler<string>? OnChange;

    public FileWatcher(string workingDirectory, string outputDirectory)
    {
        _workingDirectory = workingDirectory;
        _outputDirectory = outputDirectory;
    }

    public void Start()
    {
        // content 디렉토리 감시
        var contentPath = Path.Combine(_workingDirectory, "content");
        if (Directory.Exists(contentPath))
        {
            AddWatcher(contentPath, "*.md");
            AddWatcher(contentPath, "*.html");

            // static 폴더 감시 (모든 파일)
            var staticPath = Path.Combine(contentPath, "static");
            if (Directory.Exists(staticPath))
            {
                AddWatcher(staticPath, "*.*");
            }
        }

        // config.json 감시
        AddWatcher(_workingDirectory, "config.json");

        // Components 디렉토리 감시 (Blazor 컴포넌트)
        var componentsPath = Path.Combine(_workingDirectory, "src", "DotnetSsg", "Components");
        if (Directory.Exists(componentsPath))
        {
            AddWatcher(componentsPath, "*.razor");
            AddWatcher(componentsPath, "*.cs");
        }

        // Tailwind CSS 입력 파일 감시
        var tailwindInputPath = Path.Combine(contentPath, "static", "css", "input.css");
        if (File.Exists(tailwindInputPath))
        {
            var cssDir = Path.GetDirectoryName(tailwindInputPath);
            if (cssDir != null)
            {
                AddWatcher(cssDir, "input.css");
            }
        }
    }

    private void AddWatcher(string path, string filter)
    {
        try
        {
            var watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = filter,
                NotifyFilter = NotifyFilters.FileName
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.LastWrite
                               | NotifyFilters.CreationTime,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileChanged;
            watcher.Deleted += OnFileChanged;
            watcher.Renamed += OnFileRenamed;

            _watchers.Add(watcher);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ 파일 감시 설정 실패 ({path}): {ex.Message}");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // 출력 디렉토리 변경은 무시
        var outputFullPath = Path.GetFullPath(Path.Combine(_workingDirectory, _outputDirectory));
        var changedFullPath = Path.GetFullPath(e.FullPath);
        if (changedFullPath.StartsWith(outputFullPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // 임시 파일 무시 (Visual Studio, VS Code 등의 임시 파일)
        var fileName = Path.GetFileName(e.FullPath);
        if (fileName.StartsWith("~") ||
            fileName.StartsWith(".") ||
            e.FullPath.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("~RF") || // Visual Studio 임시 파일
            fileName.EndsWith(".swp") || // Vim 임시 파일
            fileName.EndsWith(".swo")) // Vim 임시 파일
        {
            return;
        }

        HandleFileChange(e.FullPath);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        // 출력 디렉토리 변경은 무시
        var outputFullPath = Path.GetFullPath(Path.Combine(_workingDirectory, _outputDirectory));
        var changedFullPath = Path.GetFullPath(e.FullPath);
        if (changedFullPath.StartsWith(outputFullPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        HandleFileChange(e.FullPath);
    }

    private void HandleFileChange(string fullPath)
    {
        lock (_lock)
        {
            var now = DateTime.Now;

            // Debouncing: 같은 파일이 짧은 시간에 여러 번 변경되는 것 방지
            if (_lastChangeTime.TryGetValue(fullPath, out var lastTime) &&
                (now - lastTime).TotalMilliseconds < DebounceMilliseconds)
            {
                return;
            }

            _lastChangeTime[fullPath] = now;
        }

        // 상대 경로로 변환하여 표시
        var relativePath = Path.GetRelativePath(_workingDirectory, fullPath);

        // 이벤트 발생 (비동기로 처리하기 위해 Task.Run 사용)
        Task.Run(() =>
        {
            try
            {
                OnChange?.Invoke(this, relativePath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"FileWatcher OnChange 핸들러에서 오류 발생 ('{relativePath}'): {ex}");
            }
        });
    }

    public void Dispose()
    {
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Changed -= OnFileChanged;
            watcher.Created -= OnFileChanged;
            watcher.Deleted -= OnFileChanged;
            watcher.Renamed -= OnFileRenamed;
            watcher.Dispose();
        }

        _watchers.Clear();
    }
}
