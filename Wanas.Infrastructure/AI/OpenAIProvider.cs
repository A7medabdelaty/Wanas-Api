
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Wanas.Application.Interfaces.AI;

namespace Wanas.Infrastructure.AI
{
    public class OpenAIProvider : IAIProvider
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
            //  request 
            var requestBody = new
            {
                model = "gpt-4o-mini",
                input = prompt
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Clear headers and add authentication
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            //  Call OpenAI Responses 
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/responses", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI error: {response.StatusCode}");

            var resultJson = await response.Content.ReadAsStringAsync();

            //// Debugging 
            //Console.WriteLine(" OpenAI Response JSON:");
            //Console.WriteLine(resultJson);

            //  Parse JSON
            using var doc = JsonDocument.Parse(resultJson);
            var root = doc.RootElement;

            
            if (!root.TryGetProperty("output", out var outputArray) || outputArray.GetArrayLength() == 0)
                throw new Exception("AI response missing 'output'");

            var firstOutput = outputArray[0];

            if (!firstOutput.TryGetProperty("content", out var contentArray) || contentArray.GetArrayLength() == 0)
                throw new Exception("AI response missing 'content'");

            var firstContent = contentArray[0];

            if (!firstContent.TryGetProperty("text", out var textElement))
                throw new Exception("AI response missing 'text'");

            var text = textElement.GetString();

            return text ?? "";
        }
    }
}

