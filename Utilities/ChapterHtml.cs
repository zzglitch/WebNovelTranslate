using System.Reflection;
using RazorEngine;
using RazorEngine.Templating;

namespace WebNovelTranslate.Utilities;

public class ChapterHtml
{
    public static string GetEmbeddedResourceContent(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new Exception($"Resource {resourceName} not found in assembly {assembly.FullName}");
        }
        using StreamReader reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    
    public static string ConvertToHtml(string chapterContent)
    {
        var lines = chapterContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        var model = new
        {
            ChapterTitle = lines[0],
            lines = lines.Skip(1)
        };
        
        var templateContent = GetEmbeddedResourceContent("WebNovelTranslate.Resources.chapter_template.cshtml");
        var htmlContent = Engine.Razor.RunCompile(templateContent, "templateKey", null, model);

        return htmlContent;
    }
}