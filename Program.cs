using System.Net;
using System.Text;
using CommandLine;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Html2pdf;
using WebNovelTranslate.Translator;
using WebNovelTranslate.Utilities;

namespace WebNovelTranslate;

internal class Program
{
    [Verb("download", HelpText = "Download chapters from web novel")]
    public class DownloadOptions
    {
        [Value(0, MetaName = "URL", Required = true, HelpText = "Base URL to download web novel")]
        public string BaseUrl { get; set; } = "";

        [Option('o', "out", Required = true, HelpText = "Output directory")]
        public string OutDir { get; set; } = "Novel";

        [Option('n', "number_to_download", Required = false, HelpText = "Number of chapters to download (-1 for all)")]
        public int NumberOfChaptersToProcess { get; set; } = -1;

        [Option("delay_between_downloads", Required = false, HelpText = "Milliseconds to pause between downloads")]
        public int SecondsBetweenDownloads { get; set; } = 5000;

        [Option('v', "verbose", Default = false, Required = false, HelpText = "Set output to verbose messages")]
        public bool Verbose { get; set; } = false;
    }

    [Verb("translate", HelpText = "Translate downloaded chapters into english")]
    public class TranslateOptions
    {

        [Option('o', "out", Required = true, HelpText = "Output directory")]
        public string OutDir { get; set; } = "Novel";

        [Option('g', "google", Required = false, HelpText = "Google key for translate API")]
        public string GoogleKey { get; set; } = "";

        [Option('x', "ignore", Required = false, HelpText = "Throwaway option for testing purposes")]
        public string DevNull { get; set; } = "";

        [Option('v', "verbose", Default = false, Required = false, HelpText = "Set output to verbose messages")]
        public bool Verbose { get; set; } = false;
    }

    [Verb("pdf", HelpText = "Convert translated chapters into single PDF")]
    public class PdfOptions
    {
        [Option('o', "out", Required = true, HelpText = "Output directory")]
        public string OutDir { get; set; } = "Novel";

        [Option('t', "title", Required = false, HelpText = "Title of the novel")]
        public string Title { get; set; } = "Novel Title";

        [Option('f', "filename", Required = false, HelpText = "PDF file name to generate")]
        public string OutFile { get; set; } = "novel.pdf";

        [Option('v', "verbose", Default = false, Required = false, HelpText = "Set output to verbose messages")]
        public bool Verbose { get; set; } = false;
    }

    private static async Task<ChapterDownloadResult> DownloadChapter(DownloadOptions opts, IChapterDownload chapterDownloader, string outDir, int chapterNum)
    {
        if (opts.Verbose)
            Console.Write("Download loading chapter {chapterNum}...");
        
        var result = await chapterDownloader.DownloadChapterAsync(chapterNum);
        if (result.StatusCode == HttpStatusCode.OK)
        {
            ChapterFile.WriteDownloadedChapter(outDir, result);
            if (opts.Verbose)
                Console.WriteLine("success");
        }
        else
        {
            if (opts.Verbose)
                Console.WriteLine($"failed with status code {result.StatusCode}");
        }

        // force small delay to reduce load on server
        await Task.Delay(opts.SecondsBetweenDownloads);
        return result;
    }
    
    private static async Task<int> RunDownloadAndExitCode(DownloadOptions opts)
    {
        // ensure output directory exists
        if (!Directory.Exists(opts.OutDir))
            Directory.CreateDirectory(opts.OutDir);

        // ensure the jpn download directory exists
        var downloadPath = Path.Combine(opts.OutDir, "jpn");
        if (!Directory.Exists(downloadPath))
            Directory.CreateDirectory(downloadPath);

        // Are any downloads requested?
        if (opts.NumberOfChaptersToProcess <= 0)
            return 0;
        
        // get list of already downloaded chapters
        var existingChapterFiles = ChapterFile.GetChapterFiles(downloadPath);
        var missingChapters = ChapterFile.GetMissingChapters(existingChapterFiles);

        // prep for downloads
        var download = ChapterDownloadFactory.CreateChapterDownload(opts.BaseUrl);
        if (download == null)
        {
            Console.WriteLine($"Unrecognized base URL: {opts.BaseUrl}");
            return -1;
        }

        // quota for maximum number of chapters to download
        var quotaChaptersToDownload = opts.NumberOfChaptersToProcess;
        if (quotaChaptersToDownload < 0)
            quotaChaptersToDownload = int.MaxValue;

        // download missing chapters
        foreach (var chapterNum in missingChapters)
        {
            await DownloadChapter(opts, download, downloadPath, chapterNum);

            // have we reached our quota?
            quotaChaptersToDownload -= 1;
            if (quotaChaptersToDownload <= 0)
                break;
        }

        // download remaining chapters until hitting quota or no more chapters
        var currentChapter = 0;
        if (existingChapterFiles.Count > 0)
            currentChapter = existingChapterFiles.Keys.Last();
        for (; quotaChaptersToDownload > 0; quotaChaptersToDownload -= 1)
        {
            var result = await DownloadChapter(opts, download, downloadPath, ++currentChapter);
            if (result.StatusCode != HttpStatusCode.OK)
                break;
        }

        return 0;
    }

    private static async Task<int> RunTranslateAndExitCode(TranslateOptions opts)
    {
        // ensure output directory exists
        if (!Directory.Exists(opts.OutDir))
            Directory.CreateDirectory(opts.OutDir);

        // ensure the jpn download directory exists
        var downloadPath = Path.Combine(opts.OutDir, "jpn");
        if (!Directory.Exists(downloadPath))
            Directory.CreateDirectory(downloadPath);

        // ensure the eng translation directory exists
        var translationPath = Path.Combine(opts.OutDir, "eng");
        if (!Directory.Exists(translationPath))
            Directory.CreateDirectory(translationPath);

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
        
        return 0;
    }

    private static async Task<int> RunPdfGenerateAndExitCode(PdfOptions opts)
    {
        // ensure output directory exists
        if (!Directory.Exists(opts.OutDir))
            Directory.CreateDirectory(opts.OutDir);
        
        // ensure the eng translation directory exists
        var translationPath = Path.Combine(opts.OutDir, "eng");
        if (!Directory.Exists(translationPath))
            Directory.CreateDirectory(translationPath);
        
        // get list of chapters to include in PDF
        var translatedChapterFiles = ChapterFile.GetChapterFiles(translationPath);
        
        // generate HTML version of novel
        var novelHtml = HtmlGenerator.ChapterFilesToNovelHtml(opts.Title, translatedChapterFiles);
        
        // write HTML to file, mainly for debugging
        var filenameBase = Path.GetFileNameWithoutExtension(opts.OutFile);
        var htmlFileName = Path.Combine(opts.OutDir, $"{filenameBase}.html");
        await File.WriteAllTextAsync(htmlFileName, novelHtml, Encoding.UTF8);
        
        // convert HTML to PDF
        var pdfFileName = Path.Combine(opts.OutDir, $"{filenameBase}.pdf"); 
        await using var pdfDest = File.Open(pdfFileName, FileMode.Create);
        HtmlConverter.ConvertToPdf(novelHtml, pdfDest);
        
        return 0;
    }
    
    public static async Task<int> Main(string[] args)
    {
        var result = Parser.Default.ParseArguments<DownloadOptions, TranslateOptions, PdfOptions>(args);
        return await result.MapResult(
                async (DownloadOptions opts) => await RunDownloadAndExitCode(opts),
                async (TranslateOptions opts) => await RunTranslateAndExitCode(opts),
                async (PdfOptions opts) => await RunPdfGenerateAndExitCode(opts),
                errs => Task.FromResult<int>(-1));
    }
}