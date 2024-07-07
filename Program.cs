using System.Net;
using System.Text;
using CommandLine;
using CommandLine.Text;
using WebNovelTranslate.Translator;
using WebNovelTranslate.Utilities;

namespace WebNovelTranslate;

internal class Program
{
    private static async Task<ChapterDownloadResult> DownloadChapter(Options opts, IChapterDownload chapterDownloader,
        string outDir, int chapterNum)
    {
        if (opts.Verbose) Console.Write("Download loading chapter {chapterNum}...");
        var result = await chapterDownloader.DownloadChapterAsync(chapterNum);
        if (result.StatusCode == HttpStatusCode.OK)
        {
            ChapterFile.WriteDownloadedChapter(outDir, result);
            if (opts.Verbose) Console.WriteLine("success");
        }
        else
        {
            if (opts.Verbose) Console.WriteLine($"failed with status code {result.StatusCode}");
        }

        // force small delay to reduce load on server
        await Task.Delay(opts.SecondsBetweenDownloads);
        return result;
    }

    private static async Task DownloadJapaneseChapters(Options opts, string downloadPath)
    {
        // get list of already downloaded chapters
        var existingChapterFiles = ChapterFile.GetChapterFiles(downloadPath);
        var missingChapters = ChapterFile.GetMissingChapters(existingChapterFiles);

        // prep for downloads
        var download = ChapterDownloadFactory.CreateChapterDownload(opts.BaseUrl);
        if (download == null)
        {
            Console.WriteLine($"Unrecognized base URL: {opts.BaseUrl}");
            return;
        }

        // quota for maximum number of chapters to download
        var quotaChaptersToDownload = opts.NumberOfChaptersToProcess;
        if (quotaChaptersToDownload < 0) quotaChaptersToDownload = int.MaxValue;

        // download missing chapters
        foreach (var chapterNum in missingChapters)
        {
            await DownloadChapter(opts, download, downloadPath, chapterNum);

            // have we reached our quota?
            quotaChaptersToDownload -= 1;
            if (quotaChaptersToDownload <= 0) break;
        }

        // download remaining chapters until hitting a not found.
        var currentChapter = 0;
        if (existingChapterFiles.Count > 0) currentChapter = existingChapterFiles.Keys.Last();
        for (; quotaChaptersToDownload > 0; quotaChaptersToDownload -= 1)
            await DownloadChapter(opts, download, downloadPath, ++currentChapter);
    }


    private static async Task TranslateJapaneseToEnglish(Options opts, string downloadPath, string translationPath)
    {
        // get list of downloaded and translated chapters
        var downloadedChapterFiles = ChapterFile.GetChapterFiles(downloadPath);
        var translatedChapterFiles = ChapterFile.GetChapterFiles(translationPath);

        // get list of untranslated chapters
        var untranslatedChapters = downloadedChapterFiles.Keys.Except(translatedChapterFiles.Keys).ToList();
        
        // translate each chapter
        foreach (var untranslatedChapterNum in untranslatedChapters)
        {
            var jpnLines = ChapterFile.ReadChapterFileByLine(downloadPath, untranslatedChapterNum);
            ITranslator? translator = null;
            if (!string.IsNullOrEmpty(opts.GoogleKey))
                translator = new GoogleTranslator(opts.Verbose, opts.GoogleKey);
            else
                translator = new OllamaTranslator(opts.Verbose);
            var engChapter = await translator.JapaneseToEnglishAsync(jpnLines);
            ChapterFile.WriteTranslatedChapter(translationPath, untranslatedChapterNum, engChapter);
        }
    }

    private static async Task RunOptionsAndReturnExitCode(Options opts)
    {
        // ensure output directory exists
        if (!Directory.Exists(opts.OutDir)) Directory.CreateDirectory(opts.OutDir);

        // ensure the jpn download directory exists
        var downloadPath = Path.Combine(opts.OutDir, "jpn");
        if (!Directory.Exists(downloadPath)) Directory.CreateDirectory(downloadPath);

        // ensure the eng translation directory exists
        var translationPath = Path.Combine(opts.OutDir, "eng");
        if (!Directory.Exists(translationPath)) Directory.CreateDirectory(translationPath);

        // download japanese chapters
        if (opts.NumberOfChaptersToProcess != 0) await DownloadJapaneseChapters(opts, downloadPath);

        // translate japanese chapters
        await TranslateJapaneseToEnglish(opts, downloadPath, translationPath);
        
        Environment.Exit(0);
    }

    private static void HandleParseError(ParserResult<Options> results, IEnumerable<Error> errs)
    {
        var error = HelpText.AutoBuild(results);
        Console.WriteLine(error);
    }



    public class Options
    {
        [Value(0, MetaName = "URL", Required = true, HelpText = "Base URL to download web novel")]
        public string BaseUrl { get; set; } = "";

        [Option('o', "out", Required = true, HelpText = "Output directory")]
        public string OutDir { get; set; } = "Novel";
        
        [Option('g', "google", Required = false, HelpText = "Google key for translate API")]
        public string GoogleKey { get; set; } = "";
        
        [Option('x', "devnull", Required = false, HelpText = "Throwaway option for testing purposes")]
        public string DevNull { get; set; } = "";

        [Option('n', "number_to_download", Required = false, HelpText = "Number of chapters to download (-1 for all)")]
        public int NumberOfChaptersToProcess { get; set; } = -1;

        [Option("delay_between_downloads", Required = false, HelpText = "Milliseconds to pause between downloads")]
        public int SecondsBetweenDownloads { get; set; } = 5000;

        [Option('v', "verbose", Default = false, Required = false, HelpText = "Set output to verbose messages")]
        public bool Verbose { get; set; } = false;
    }
    
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        var results = Parser.Default.ParseArguments<Options>(args);
        if (results.Tag == ParserResultType.Parsed)
            await results.WithParsedAsync(async opts => await RunOptionsAndReturnExitCode(opts));
        else
            results.WithNotParsed(errs => HandleParseError(results, errs));
    }
}