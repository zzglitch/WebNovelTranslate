using System.Net;
using System.Text;
using CommandLine;
using CommandLine.Text;
using WebNovelTranslate.Translator;
using WebNovelTranslate.Utilities;

namespace WebNovelTranslate
{
    class Program
    {
        public class Options
        {
            [Value(0, MetaName = "URL", Required = true, HelpText = "Base URL to download web novel")]
            public string BaseUrl { get; set; } = "";

            [Option('o', "out", Required = true, HelpText = "Output directory")]
            public string OutDir { get; set; } = "Novel";
            
            [Option('n', "number_to_download", Required = false, HelpText = "How many chapters to download (-1 for all)")]
            public int NumberOfChaptersToProcess { get; set; } = -1;
            
            [Option( "delay_between_downloads", Required = false, HelpText = "How many milliseconds to pause between downloads")]
            public int SecondsBetweenDownloads { get; set; } = 2000;
            
            [Option('v', "verbose", Default = false, Required = false, HelpText = "Set output to verbose messages")]
            public bool Verbose { get; set; } = false;
        }
        
        static async Task DownloadJapaneseChapters(Options opts, string downloadPath)
        {
            // ensure base url ends with '/'
            if (!opts.BaseUrl.EndsWith('/'))
            {
                opts.BaseUrl += '/';
            }
            
            // get list of already downloaded chapters
            var existingChapterFiles = Utilities.ChapterFile.GetChapterFiles(downloadPath);
            var missingChapters = Utilities.ChapterFile.GetMissingChapters(existingChapterFiles);

            // prep for downloads
            var download = ChapterDownloadFactory.CreateChapterDownload(opts.BaseUrl);
            if (download == null)
            {
                Console.WriteLine($"Unrecognized base URL: {opts.BaseUrl}");
                return;
            }
            
            // quota for maximum number of chapters to download
            var quotaChaptersToDownload = opts.NumberOfChaptersToProcess;
            if (quotaChaptersToDownload < 0)
            {
                quotaChaptersToDownload = int.MaxValue;
            }

            // download missing chapters
            foreach (var chapterNum in missingChapters)
            {
                var result = await download.DownloadChapterAsync(chapterNum);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    ChapterFile.WriteDownloadedChapter(downloadPath, result);
                }
                else
                {
                    Console.WriteLine($"Failed to download chapter {chapterNum} with status code {result.StatusCode}");
                }
            
                // have we reached our quota?
                quotaChaptersToDownload -= 1;
                if (quotaChaptersToDownload <= 0)
                {
                    break;
                }
            
                // small delay between page downloads to reduce load on server
                await Task.Delay(opts.SecondsBetweenDownloads);
            }

            // download remaining chapters until hitting a not found.
            var currentChapter = 0;
            if (existingChapterFiles.Count > 0)
            {
                currentChapter = existingChapterFiles.Keys.Last();
            }
            while (quotaChaptersToDownload > 0)
            {
                currentChapter += 1;
                var result = await download.DownloadChapterAsync(currentChapter);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    ChapterFile.WriteDownloadedChapter(downloadPath, result);
                    quotaChaptersToDownload -= 1;
                }
                else
                {
                    if (opts.Verbose)
                    {
                        Console.WriteLine($"Failed to download chapter {currentChapter} with status code {result.StatusCode}");
                    }
                    break;
                }
            
                // small delay between page downloads to reduce load on server
                await Task.Delay(opts.SecondsBetweenDownloads);
            }
        }


        static async Task TranslateJapaneseToEnglish(Options opts, string downloadPath, string translationPath)
        {
            // get list of downloaded and translated chapters
            var downloadedChapterFiles = Utilities.ChapterFile.GetChapterFiles(downloadPath);
            var translatedChapterFiles = Utilities.ChapterFile.GetChapterFiles(translationPath);
            
            // get list of untranslated chapters
            var untranslatedChapters = downloadedChapterFiles.Keys.Except(translatedChapterFiles.Keys).ToList();
            foreach (var untranslatedChapterNum in untranslatedChapters)
            {
                // load japanese chapter by lines
                var lines = ChapterFile.ReadChapterFileByLine(downloadPath, untranslatedChapterNum);

                // translate line by line due to Ollama size limitations
                ITranslator translator = new OllamaTranslator(opts.Verbose);
                StringBuilder translatedChapter = new StringBuilder();
                foreach (var line in lines)
                {
                    var translatedLine = await translator.JapaneseToEnglishAsync(line);
                    translatedChapter.AppendLine(translatedLine);
                }
                
                // write english chapter
                ChapterFile.WriteTranslatedChapter(translationPath, untranslatedChapterNum, translatedChapter.ToString());
            }
        }
        
        static async Task RunOptionsAndReturnExitCode(Options opts)
        {
            // ensure output directory exists
            if (!Directory.Exists(opts.OutDir))
            {
                Directory.CreateDirectory(opts.OutDir);
            }
            
            // ensure the jpn download directory exists
            var downloadPath = Path.Combine(opts.OutDir, "jpn");
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }
            
            // ensure the eng translation directory exists
            var translationPath = Path.Combine(opts.OutDir, "eng");
            if (!Directory.Exists(translationPath))
            {
                Directory.CreateDirectory(translationPath);
            }
            
            // download japanese chapters
            if (opts.NumberOfChaptersToProcess != 0)
            {
                await DownloadJapaneseChapters(opts, downloadPath);

            }
            
            // translate japanese chapters
            await TranslateJapaneseToEnglish(opts, downloadPath, translationPath);
        }

        static void HandleParseError(ParserResult<Options> results, IEnumerable<Error> errs)
        {
            var error = HelpText.AutoBuild(results);
            Console.WriteLine(error);
        }
        
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var results= Parser.Default.ParseArguments<Options>(args);
            if (results.Tag == ParserResultType.Parsed)
            {
                await results.WithParsedAsync(async opts => await RunOptionsAndReturnExitCode(opts));
            }
            else
            {
                results.WithNotParsed(errs => HandleParseError(results, errs));
            }
        }
    }
}
