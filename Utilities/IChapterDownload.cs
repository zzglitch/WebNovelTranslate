using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace WebNovelTranslate.Utilities
{
    public class ChapterDownloadResult
    {
        public HttpStatusCode StatusCode { get; init; }
        public int ChapterNumber { get; init; } = -1;
        public string ChapterTitle { get; init; } = "";
        public string Content { get; init; } = "";
    }
    
    public interface IChapterDownload
    {
        public Task<ChapterDownloadResult> DownloadChapterAsync(int chapterNum);
    }
    
    public class SyosetuChapterDownload : IChapterDownload
    {
        private string _url;

        public SyosetuChapterDownload(string baseUrl)
        {
            // ensure base url ends with '/'
            _url = baseUrl;
            if (!_url.EndsWith('/'))
            {
                _url += '/';
            }
        }
            
        public async Task<ChapterDownloadResult> DownloadChapterAsync(int chapterNum)
        {
            // download the chapter
            var url = $"{_url}{chapterNum}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return new ChapterDownloadResult { StatusCode = response.StatusCode, ChapterNumber = chapterNum };
            }
            
            // prep HTML parsing
            HtmlDocument doc = new HtmlDocument();
            var pageContent = await response.Content.ReadAsStringAsync();
            doc.LoadHtml(pageContent);
            
            // extract chapter title
            var chapterTitle = doc.DocumentNode.SelectSingleNode("//p[@class='novel_subtitle']")?.InnerText ?? "No Chapter Title";
            
            // extract chapter text
            var bodyNode = doc.DocumentNode.SelectSingleNode("//div[@id='novel_honbun']");
            if (bodyNode == null)
            {
                return new ChapterDownloadResult { StatusCode = HttpStatusCode.NoContent, ChapterNumber = chapterNum };
            }
            var bodyBuilder = new StringBuilder();
            foreach (var lineNode in bodyNode.ChildNodes)
            {
                if (lineNode is not { Name: "p" })
                {
                    continue;
                }
                if (lineNode.FirstChild is { Name: "br" })
                {
                    bodyBuilder.AppendLine(); 
                }
                else if (!string.IsNullOrEmpty(lineNode.InnerText))
                {
                    bodyBuilder.AppendLine(lineNode.InnerText.Trim());
                }
            }
            
            return new ChapterDownloadResult { StatusCode = HttpStatusCode.OK, ChapterNumber = chapterNum, ChapterTitle = chapterTitle, Content = bodyBuilder.ToString()};
        }
    }

    public static class ChapterDownloadFactory
    {
        public static IChapterDownload? CreateChapterDownload(string baseUrl)
        {
            if (baseUrl.StartsWith("https://ncode.syosetu.com/"))
            {
                return new SyosetuChapterDownload(baseUrl);   
            }
            if (baseUrl.StartsWith("https://novel18.syosetu.com/"))
            {
                return new SyosetuChapterDownload(baseUrl);   

            }

            return null;
        }
    }
}

