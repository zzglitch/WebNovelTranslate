namespace WebNovelTranslate.Translator;

public interface ITranslator
{
    public Task<string> JapaneseToEnglishAsync(string jpnStr);
}