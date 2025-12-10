using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Wanas.Application.Interfaces;

namespace Wanas.Application.Services
{
    public class GeminiEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiEmbeddingService> _logger;
        private readonly string _apiKey;
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta";
        private const string Model = "models/text-embedding-004";

        public GeminiEmbeddingService(HttpClient httpClient, ILogger<GeminiEmbeddingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Read API key from environment variable
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Gemini API Key is not configured. Set GEMINI_API_KEY environment variable.");
            }

            _logger.LogInformation("Gemini Embedding Service initialized with model: {Model}", Model);
        }

        public async Task<float[]> GenerateEmbeddingsAsync(string text)
        {
            try
            {
                _logger.LogInformation("Generating Gemini embeddings for text of length {Length}", text.Length);

                var request = new
                {
                    model = Model,
                    content = new
                    {
                        parts = new[] 
                        {
                            new { text = text }
                        }
                    }
                };

                var url = $"{BaseUrl}/{Model}:embedContent?key={_apiKey}";
                var response = await _httpClient.PostAsJsonAsync(url, request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);

                var embeddingArray = document.RootElement
                    .GetProperty("embedding")
                    .GetProperty("values")
                    .EnumerateArray()
                    .Select(x => (float)x.GetDouble())
                    .ToArray();

                _logger.LogInformation("Successfully generated {Count} dimensional embedding", embeddingArray.Length);
                return embeddingArray;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate Gemini embeddings");
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
}
