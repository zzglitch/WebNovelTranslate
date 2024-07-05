using System.Collections;
using System.Text;

namespace WebNovelTranslate.Utilities
{
    public static class ChapterFile
    {
        public static SortedDictionary<int,string> GetChapterFiles(string dir)
        {
            var result = new SortedDictionary<int, string>();
            var prefix = Path.Combine(dir, "chapter-");
            foreach (var file in Directory.GetFiles(dir, "chapter-*.txt"))
            {
                if (!file.StartsWith(prefix))
                {
                    continue;
                }
                if (int.TryParse(file.AsSpan(prefix.Length, file.Length - prefix.Length - 4), out var chapter))
                {
                    result.Add(chapter, file);
                }
            }
            return result;
        }

        public static HashSet<int> GetMissingChapters(SortedDictionary<int,string> chapters)
        {
            var missingKeys = new HashSet<int>();
            if (chapters.Count <= 0)
            {
                return missingKeys;
            }

            var minKey = chapters.Keys.First();
            var maxKey = chapters.Keys.Last();
            for (var key = minKey; key <= maxKey; key++)
            {
                if (!chapters.ContainsKey(key))
                {
                    missingKeys.Add(key);
                }
            }

            return missingKeys;
        }

        public static void WriteDownloadedChapter(string outDir, ChapterDownloadResult chapter)
        {
            var chapterFileName = Path.Combine(outDir, $"chapter-{chapter.ChapterNumber:D4}.txt");
            var builder = new StringBuilder();
            builder.AppendLine(chapter.ChapterTitle);
            builder.AppendLine();
            builder.AppendLine(chapter.Content);
            File.WriteAllText(chapterFileName, builder.ToString(), Encoding.UTF8);
        }

        public static void WriteTranslatedChapter(string outDir, int chapterNum, string? chapterBody)
        {
            var chapterFileName = Path.Combine(outDir, $"chapter-{chapterNum:D4}.txt");
            File.WriteAllText(chapterFileName, chapterBody, Encoding.UTF8);
        }

        public static List<string> ReadChapterFileByLine(string inDir, int chapterNum)
        {
            var chapterFileName = Path.Combine(inDir, $"chapter-{chapterNum:D4}.txt");
            return File.ReadAllLines(chapterFileName, Encoding.UTF8).ToList();
        }
    }
}
