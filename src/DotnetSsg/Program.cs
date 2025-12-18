using DotnetSsg.Models;
using DotnetSsg.Services;
using System.Diagnostics;

// Helper to clean and recreate a directory
void CleanAndCreateDirectory(string path)
{
    if (Directory.Exists(path))
    {
        Directory.Delete(path, true);
    }
    Directory.CreateDirectory(path);
}

// Main execution
var stopwatch = Stopwatch.StartNew();
try
{
    Console.WriteLine("Static Site Generation starting...");

    // 0. Clean output directory
    Console.WriteLine("\n0. Cleaning output directory...");
    CleanAndCreateDirectory("output");
    Console.WriteLine("   > Output directory cleaned.");

    // 1. Load Config
    Console.WriteLine("\n1. Loading site configuration...");
    var configLoader = new ConfigLoader();
    var siteConfig = await configLoader.LoadConfigAsync();
    Console.WriteLine($"   > Site Title: {siteConfig.Title}");

    // 2. Scan for content
    Console.WriteLine("\n2. Scanning for markdown files...");
    var fileScanner = new FileScanner();
    var markdownFiles = fileScanner.Scan("content", "md").ToList();
    Console.WriteLine($"   > Found {markdownFiles.Count} markdown files.");

    // 3. Copy static files
    Console.WriteLine("\n3. Copying static files...");
    var staticFileCopier = new StaticFileCopier();
    staticFileCopier.Copy("content/static", "output/static");
    Console.WriteLine("   > Static files copied.");


    // 4. Parse Markdown files in parallel
    Console.WriteLine("\n4. Parsing markdown files...");
    var markdownParser = new MarkdownParser();
    var contentItems = new List<ContentItem>();
    var parseTasks = markdownFiles.Select(async file =>
    {
        try
        {
            var item = await markdownParser.ParseAsync(file);
            lock (contentItems)
            {
                contentItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] Failed to parse '{file}': {ex.Message}");
        }
    });
    await Task.WhenAll(parseTasks);
    Console.WriteLine($"   > Parsed {contentItems.Count} content items.");

    var posts = contentItems.OfType<Post>().ToList();
    var pages = contentItems.OfType<Page>().ToList();
    Console.WriteLine($"   > Posts: {posts.Count}, Pages: {pages.Count}");


    // 5. Generate individual HTML pages in parallel
    Console.WriteLine("\n5. Generating individual HTML pages...");
    var templateRenderer = new TemplateRenderer();
    var htmlGenerator = new HtmlGenerator(templateRenderer);
    await Parallel.ForEachAsync(contentItems.Where(i => !Path.GetFileName(i.SourcePath).Equals("index.md", StringComparison.OrdinalIgnoreCase)), async (item, token) =>
    {
        try
        {
            await htmlGenerator.GenerateAsync(item, siteConfig);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] Failed to generate HTML for '{item.SourcePath}': {ex.Message}");
        }
    });
    Console.WriteLine("   > Individual pages generated.");


    // 6. Generate aggregate pages (Index, Tags)
    Console.WriteLine("\n6. Generating aggregate pages...");
    var indexGenerator = new IndexGenerator(templateRenderer);
    await indexGenerator.GenerateAsync(contentItems, siteConfig);
    Console.WriteLine("   > Index page generated.");

    var tagArchiveGenerator = new TagArchiveGenerator(templateRenderer);
    await tagArchiveGenerator.GenerateAsync(contentItems, siteConfig);
    Console.WriteLine("   > Tag archive pages generated.");

    stopwatch.Stop();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n✅ Success! Static site generation complete in {stopwatch.ElapsedMilliseconds}ms.");
    Console.ResetColor();
}
catch (Exception ex)
{
    stopwatch.Stop();
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"\n❌ An unhandled error occurred: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    Console.ResetColor();
    Console.WriteLine($"\nStatic site generation failed after {stopwatch.ElapsedMilliseconds}ms.");
    Environment.ExitCode = 1;
}