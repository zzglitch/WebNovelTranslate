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

    public async Task<string> JapaneseToEnglishAsync(string jpnStr)
    {
        if (string.IsNullOrEmpty(jpnStr) || string.IsNullOrWhiteSpace(jpnStr))
            return "";

        var prompt = $"translate following japanese to english with no added prefix or explanation in the output: {jpnStr.Trim()}";
        var response = await _ollama.GetCompletion(prompt, null);
        var line = response.Response;
        return !string.IsNullOrEmpty(line) ? line : "";
    }
}