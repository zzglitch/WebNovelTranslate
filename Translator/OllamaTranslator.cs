using System.Text;
using OllamaSharp;

namespace WebNovelTranslate.Translator;

internal class OllamaTranslator : ITranslator
{
    private bool _isVerbose;
    private readonly OllamaApiClient _ollama;


    public OllamaTranslator(bool isVerbose = false)
    {
        _isVerbose = isVerbose;
        _ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
        _ollama.SelectedModel = "qwen2:latest";
    }

    public async Task<string> JapaneseToEnglishAsync(List<string>? jpnStrs)
    {
        if (jpnStrs == null || jpnStrs.Count == 0)
            return "";
        var builder = new StringBuilder();
        foreach (var jpnStr in jpnStrs)
        {
            if (string.IsNullOrEmpty(jpnStr) || string.IsNullOrWhiteSpace(jpnStr))
            {
                builder.AppendLine("");
                continue;
            }
            var prompt = $"translate following japanese to english with no added prefix or explanation in the output: {jpnStr.Trim()}";
            var response = await _ollama.GetCompletion(prompt, null);
            var engStr = response.Response ?? "";
            builder.AppendLine(engStr);
        }
        return builder.ToString();
    }
}