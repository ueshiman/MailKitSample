using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace MailKitSample.Services
{
    public class PromptDispatcher : IPromptDispatcher
    {
        private class PromptEntry
        {
            public string Prompt { get; init; } = string.Empty;
            public int Weight { get; init; } = 1;
        }

        private readonly List<PromptEntry> _entries = new();
        private readonly Random _random = new();
        private readonly string _defaultPromptJsonPath = "Prompt.json";
        private readonly ILogger<PromptDispatcher> _logger;
        private readonly IAiMailBuilder _aiMailBuilder;

        public PromptDispatcher(ILogger<PromptDispatcher> logger, IAiMailBuilder aiMailBuilder)
        {
            _aiMailBuilder = aiMailBuilder;
            _logger = logger;
            string promptJsonPath = _defaultPromptJsonPath;
            if (!File.Exists(promptJsonPath))
            {
                logger.LogError("Prompt.json が見つかりません: {Path}", promptJsonPath);
                throw new FileNotFoundException("Prompt.json が見つかりません", promptJsonPath);
            }

            var json = File.ReadAllText(promptJsonPath);
            var node = JsonNode.Parse(json) as JsonObject;
            if (node == null)
            {
                logger.LogError("Prompt.json のパースに失敗しました");
                throw new InvalidOperationException("Prompt.json のパースに失敗しました");
            }

            foreach (var kv in node)
            {
                if (kv.Value is JsonObject obj)
                {
                    var prompt = obj["prompt"]?.ToString();
                    var weight = obj["weight"]?.GetValue<int>() ?? 1;
                    if (!string.IsNullOrWhiteSpace(prompt)) _entries.Add(new PromptEntry { Prompt = prompt, Weight = weight });
                    
                }
            }

            if (_entries.Count == 0)
            {
                string message = "Prompt.json に有効なプロンプトがありません";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }
        }

        public string GetWeightedRandomPrompt()
        {
            int totalWeight = _entries.Sum(e => e.Weight);
            int r = _random.Next(1, totalWeight + 1);
            int sum = 0;
            foreach (var entry in _entries)
            {
                sum += entry.Weight;
                if (r <= sum) return entry.Prompt;
            }
            // Fallback（理論上到達しない）
            return _entries.Last().Prompt;
        }

        public async Task<string?> GenerateRandomMessageAsync()
        {
            var prompt = GetWeightedRandomPrompt();
            var result = await _aiMailBuilder.GenerateMessage(prompt);
            if (result == null)
            {
                _logger.LogWarning("AI メール生成結果が null です。プロンプト: {Prompt}", prompt);
                throw new InvalidOperationException($"AI メール生成結果が null です。プロンプト: {prompt}");
            }
            return result;
        }
    }
}