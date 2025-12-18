using Scriban;
using Scriban.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DotnetSsg.Services;

public class TemplateRenderer
{
    private readonly Dictionary<string, Template> _templateCache = new();

    public async Task<string> RenderAsync(string templatePath, object model)
    {
        var template = await GetOrLoadTemplateAsync(templatePath);
        
        var context = new TemplateContext
        {
            // Use a renamer to keep C# PascalCase naming convention, which is cleaner.
            MemberRenamer = member => member.Name
        };

        var scriptObject = new ScriptObject();
        scriptObject.Import(model);
        context.PushGlobal(scriptObject);

        return await template.RenderAsync(context);
    }

    private async Task<Template> GetOrLoadTemplateAsync(string templatePath)
    {
        if (_templateCache.TryGetValue(templatePath, out var cachedTemplate))
        {
            return cachedTemplate;
        }

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}");
        }

        var templateContent = await File.ReadAllTextAsync(templatePath);
        
        // Use ParseLiquid to enable full Liquid syntax compatibility, which includes
        // control flow statements like {% if %} and {% for %}.
        var template = Template.ParseLiquid(templateContent, templatePath);
        
        _templateCache[templatePath] = template;

        return template;
    }
}