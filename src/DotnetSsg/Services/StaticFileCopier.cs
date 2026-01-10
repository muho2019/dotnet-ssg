namespace DotnetSsg.Services;

public class StaticFileCopier : IStaticFileCopier
{
    public void Copy(string sourceDirectory, string destinationDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            Console.WriteLine($"정보: 소스 디렉토리 '{sourceDirectory}'가 존재하지 않습니다. 복사할 내용이 없습니다.");
            return;
        }

        var files = Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            var destinationPath = Path.Combine(destinationDirectory, relativePath);

            var destinationFileDirectory = Path.GetDirectoryName(destinationPath);
            if (destinationFileDirectory != null)
            {
                Directory.CreateDirectory(destinationFileDirectory);
            }

            File.Copy(file, destinationPath, true);
        }
    }
}
