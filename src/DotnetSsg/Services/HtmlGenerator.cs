using DotnetSsg.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSsg.Services;

/// <summary>
/// Temporary placeholder for HtmlGenerator.
/// Will be replaced with BlazorRenderer in step 5.
/// </summary>
public class HtmlGenerator
{
    private readonly TemplateRenderer _templateRenderer;

    public HtmlGenerator(TemplateRenderer templateRenderer)
    {
        _templateRenderer = templateRenderer;
    }

    public async Task GenerateAsync(ContentItem contentItem, SiteConfig siteConfig)
    {
        // Placeholder implementation for now
        // Real implementation will come in step 5 with BlazorRenderer
        await Task.CompletedTask;
    }
}
