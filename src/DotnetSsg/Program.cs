using DotnetSsg.Models;
using DotnetSsg.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

var stopwatch = Stopwatch.StartNew();

// 1. Load Config
Console.WriteLine("1. Loading site configuration...");
var configLoader = new ConfigLoader();
var siteConfig = await configLoader.LoadConfigAsync();
Console.WriteLine($"   Title: {siteConfig.Title}");

// 2. Scan for content
Console.WriteLine("\n2. Scanning for markdown files...");
var fileScanner = new FileScanner();
var markdownFiles = fileScanner.Scan("content", "md").ToList();
Console.WriteLine($"   Found {markdownFiles.Count} markdown files.");

// 3. Copy static files
Console.WriteLine("\n3. Copying static files...");
var staticFileCopier = new StaticFileCopier();
staticFileCopier.Copy("content/static", "output/static");

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
        Console.Error.WriteLine($"Error parsing '{file}': {ex.Message}");
    }
});
await Task.WhenAll(parseTasks);
Console.WriteLine($"   Parsed {contentItems.Count} content items.");

// 5. Generate individual HTML pages in parallel
Console.WriteLine("\n5. Generating individual HTML pages...");
var templateRenderer = new TemplateRenderer();
var htmlGenerator = new HtmlGenerator(templateRenderer);
await Parallel.ForEachAsync(contentItems.Where(i => i.SourcePath != "content/index.md"), async (item, token) =>
{
    try
    {
        await htmlGenerator.GenerateAsync(item, siteConfig);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error generating HTML for '{item.SourcePath}': {ex.Message}");
    }
});

// 6. Generate aggregate pages (Index, Tags)
Console.WriteLine("\n6. Generating aggregate pages...");
var indexGenerator = new IndexGenerator(templateRenderer);
await indexGenerator.GenerateAsync(contentItems, siteConfig);

var tagArchiveGenerator = new TagArchiveGenerator(templateRenderer);
await tagArchiveGenerator.GenerateAsync(contentItems, siteConfig);

stopwatch.Stop();
Console.WriteLine($"\nStatic site generation complete in {stopwatch.ElapsedMilliseconds}ms.");
