namespace DotnetSsg.Services;

public class StaticFileCopier : IStaticFileCopier
{
    public void Copy(string sourceDirectory, string destinationDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            Console.WriteLine($"Info: Source directory '{sourceDirectory}' does not exist. Nothing to copy.");
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
