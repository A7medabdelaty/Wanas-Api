using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Wanas.Application.Interfaces.AI;

namespace Wanas.Infrastructure.AI
{
    public class GroqProvider :IAIProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GroqProvider(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            _apiKey = config["AI:ApiKey"]
                     ?? throw new ArgumentNullException("Groq ApiKey missing");
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            var request = new
            {
                model = "llama3-70b-8192",  
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful AI assistant." },
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                content
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Groq API Error: {response.StatusCode}");

            var resultJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(resultJson);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return text ?? "";
        }
        }
}
