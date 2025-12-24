using System.Threading.Tasks;

namespace DotnetSsg.Services;

/// <summary>
/// Temporary placeholder for TemplateRenderer.
/// Will be replaced with BlazorRenderer in step 5.
/// </summary>
public class TemplateRenderer
{
    public async Task<string> RenderAsync(string templatePath, object? model)
    {
        // Placeholder implementation for now
        // Real implementation will come in step 5 with BlazorRenderer
        await Task.CompletedTask;
        return string.Empty;
    }
}
