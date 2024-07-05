using System.Net;

namespace WebNovelTranslate.Utilities;

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

public static class ChapterDownloadFactory
{
    public static IChapterDownload? CreateChapterDownload(string baseUrl)
    {
        var url = baseUrl;
        if (!url.EndsWith('/')) url += '/';

        if (baseUrl.StartsWith("https://ncode.syosetu.com/")) return new SyosetuChapterDownload(url);
        if (baseUrl.StartsWith("https://novel18.syosetu.com/")) return new SyosetuChapterDownload(url);
        return null;
    }
}