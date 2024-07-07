namespace WebNovelTranslate.Translator;

public interface ITranslator
{
    public Task<string> JapaneseToEnglishAsync(List<string>? jpnStrs);
}