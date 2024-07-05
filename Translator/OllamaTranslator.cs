using System.Text;
using OllamaSharp;

namespace WebNovelTranslate.Translator
{
    class OllamaTranslator : ITranslator
    {
        private OllamaApiClient _ollama;
        private bool _isVerbose;

    
        public OllamaTranslator( bool isVerbose = false)
        {
            _isVerbose = isVerbose;
            _ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
            _ollama.SelectedModel = "qwen2:latest";
        }
    
        public async Task<string> JapaneseToEnglishAsync(string jpnStr)
        {
            if (string.IsNullOrEmpty(jpnStr) || string.IsNullOrWhiteSpace(jpnStr))
            {
                return "";
            }
            
            var prompt = $"japanese to english with no prefix and no explanation: {jpnStr.Trim()}";
            var response = await _ollama.GetCompletion(prompt, null);
            var line = response.Response;
            return !string.IsNullOrEmpty(line) ? line : "";
        }
    }
}
