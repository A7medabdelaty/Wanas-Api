using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Wanas.Application.Interfaces;

namespace Wanas.Application.Services
{
    public class OpenAIEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIConfig _config;

        public OpenAIEmbeddingService(HttpClient httpClient, IOptions<OpenAIConfig> config)
        {
            _httpClient = httpClient;
            _config = config.Value;

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
        }

        public async Task<float[]> GenerateEmbeddingsAsync(string text)
        {
            var request = new
            {
                input = text,
                model = "text-embedding-3-small"
            };

            var response = await _httpClient.PostAsJsonAsync($"{_config.BaseUrl}/embeddings", request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(content);

            var embeddingArray = document.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => (float)x.GetDouble())
                .ToArray();

            return embeddingArray;
        }

        public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        {
            var embeddings = new List<float[]>();
            foreach (var text in texts)
            {
                embeddings.Add(await GenerateEmbeddingsAsync(text));
            }
            return embeddings;
        }
    }
    public class OpenAIConfig
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    }
}
