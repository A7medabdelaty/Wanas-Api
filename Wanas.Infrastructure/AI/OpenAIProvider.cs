
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Wanas.Application.Interfaces.AI;

namespace Wanas.Infrastructure.AI
{
    public class OpenAIProvider:IAIProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        public OpenAIProvider(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            _apiKey = config["AI:ApiKey"] 
                ?? throw new ArgumentNullException("AI ApiKey is missing");
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            var requestBody = new
            {
                model = "gpt-4o-mini",
                input = prompt
            };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/responses", content);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI error: {response.StatusCode}");

            var resultJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultJson);

            var text = doc.RootElement
                         .GetProperty("output_text")
                         .GetString();
            return text ?? "";

        }
    }
}
