using System.Reflection;
using RazorEngine;
using RazorEngine.Templating;

namespace WebNovelTranslate.Utilities;

public class HtmlGenerator
{
    public class ChapterModel
    {
        public string? ChapterTitle { get; set; }
        public IEnumerable<string>? Lines { get; set; }
    }
    
    public class NovelModel
    {
        public string? NovelTitle { get; set; }
        public IEnumerable<ChapterModel>? Chapters { get; set; }
    }
    
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
        
        var model = new ChapterModel
        {
            ChapterTitle = lines[0],
            Lines = lines.Skip(1)
        };
        
        var templateContent = GetEmbeddedResourceContent("WebNovelTranslate.Resources.chapter_template.cshtml");
        var htmlContent = Engine.Razor.RunCompile(templateContent, "chapterTempalte", typeof(ChapterModel), model);

        return htmlContent;
    }
    
    public static string ChapterFilesToNovelHtml(string novelTitle, SortedDictionary<int,string> chapterFilenames)
    {
        var novelModel = new NovelModel
        {
            NovelTitle = novelTitle,
            Chapters = chapterFilenames.Select(kvp =>
            {
                var lines = File.ReadAllLines(kvp.Value);
                return new ChapterModel
                {
                    ChapterTitle = lines[0],
                    Lines = lines.Skip(1)
                };
            })
        };
        
        var templateContent = GetEmbeddedResourceContent("WebNovelTranslate.Resources.novel_template.cshtml");
        var htmlContent = Engine.Razor.RunCompile(templateContent, "novelTempalte", typeof(NovelModel), novelModel);

        return htmlContent;
    }
}