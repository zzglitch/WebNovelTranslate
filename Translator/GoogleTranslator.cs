using System.Text;
using Google.Cloud.Translation.V2;

namespace WebNovelTranslate.Translator;

public class GoogleTranslator(bool isVerbose = false, string? apiKey = null) : ITranslator
{
    private bool _isVerbose = isVerbose;
    private string? _apiKey = apiKey;
    
    public async Task<string> JapaneseToEnglishAsync(List<string>? jpnStrs)
    {
        if (jpnStrs == null || jpnStrs.Count <= 0)
            return "";
        
        var client = TranslationClient.CreateFromApiKey(_apiKey);
        var builder = new StringBuilder();
        jpnStrs.ForEach(s => builder.AppendLine(s));
        var response = await client.TranslateTextAsync(builder.ToString(), "en", "ja");
        return response.TranslatedText;
    }
}