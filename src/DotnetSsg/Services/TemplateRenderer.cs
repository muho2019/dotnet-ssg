using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace DotnetSsg.Services
{
    public class FileSystemTemplateLoader : ITemplateLoader
    {
        private readonly string _basePath;

        public FileSystemTemplateLoader(string basePath)
        {
            _basePath = basePath;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            return Path.Combine(_basePath, templateName);
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return File.ReadAllText(templatePath);
        }

        public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return await File.ReadAllTextAsync(templatePath);
        }
    }

    public class TemplateRenderer
    {
        private readonly Dictionary<string, Template> _templateCache = new();

        public async Task<string> RenderAsync(string templatePath, object model)
        {
            var template = await GetOrLoadTemplateAsync(templatePath);
            
            var context = new TemplateContext
            {
                MemberRenamer = member => member.Name,
                TemplateLoader = new FileSystemTemplateLoader("templates")
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
            
            var template = Template.ParseLiquid(templateContent, templatePath);
            
            _templateCache[templatePath] = template;

            return template;
        }
    }
}