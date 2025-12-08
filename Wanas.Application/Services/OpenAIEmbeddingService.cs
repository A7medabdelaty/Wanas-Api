using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wanas.Application.Interfaces;

namespace Wanas.Application.Services
{
    public class OpenAIEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIConfig _config;
        private readonly ILogger<OpenAIEmbeddingService> _logger;

        public OpenAIEmbeddingService(HttpClient httpClient, IOptions<OpenAIConfig> config, ILogger<OpenAIEmbeddingService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;

            // Always use environment variable (loaded from .env in dev, or system env in prod)
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("OpenAI API Key is not configured. Set OPENAI_API_KEY environment variable.");
            }

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _logger.LogInformation("OpenAI Embedding Service initialized with base URL: {BaseUrl}", _config.BaseUrl);
        }

        public async Task<float[]> GenerateEmbeddingsAsync(string text)
        {
            try
            {
                _logger.LogInformation("Generating OpenAI embeddings for text of length {Length}", text.Length);
                
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

                _logger.LogInformation("Successfully generated {Count} dimensional embedding", embeddingArray.Length);
                return embeddingArray;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate OpenAI embeddings");
                throw;
            }
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
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    }
}
