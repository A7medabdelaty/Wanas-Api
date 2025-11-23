
//using Microsoft.Extensions.Configuration;
//using System.Text;
//using System.Text.Json;
//using Wanas.Application.Interfaces.AI;

//namespace Wanas.Infrastructure.AI
//{
//    public class OpenAIProvider:IAIProvider
//    {
//        private readonly HttpClient _httpClient;
//        private readonly string _apiKey;
//        public OpenAIProvider(HttpClient httpClient, IConfiguration config)
//        {
//            _httpClient = httpClient;

//            _apiKey = config["AI:ApiKey"] 
//                ?? throw new ArgumentNullException("AI ApiKey is missing");
//        }

//        public async Task<string> GenerateTextAsync(string prompt)
//        {
//            var requestBody = new
//            {
//                model = "gpt-4o-mini",
//                input = prompt
//            };
//            var json = JsonSerializer.Serialize(requestBody);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            _httpClient.DefaultRequestHeaders.Clear();

//            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

//            var response = await _httpClient.PostAsync("https://api.openai.com/v1/responses", content);
//            if (!response.IsSuccessStatusCode)
//                throw new Exception($"OpenAI error: {response.StatusCode}");

//            var resultJson = await response.Content.ReadAsStringAsync();
//            using var doc = JsonDocument.Parse(resultJson);

//            var text = doc.RootElement
//                         .GetProperty("output_text")
//                         .GetString();
//            return text ?? "";

//        }
//    }
//}




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
            // 1️⃣ Build the request body for OpenAI Responses API
            var requestBody = new
            {
                model = "gpt-4o-mini",
                input = prompt
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 2️⃣ Clear headers and add authentication
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            // 3️⃣ Call OpenAI Responses API
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/responses", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI error: {response.StatusCode}");

            // 4️⃣ Read raw JSON response
            var resultJson = await response.Content.ReadAsStringAsync();

            // 🔍 Debugging — print full response JSON
            Console.WriteLine("🔍 OpenAI Response JSON:");
            Console.WriteLine(resultJson);

            // 5️⃣ Parse JSON
            using var doc = JsonDocument.Parse(resultJson);
            var root = doc.RootElement;

            // 6️⃣ Navigate to: output → content → text
            //    With safety checks
            if (!root.TryGetProperty("output", out var outputArray) || outputArray.GetArrayLength() == 0)
                throw new Exception("AI response missing 'output'");

            var firstOutput = outputArray[0];

            if (!firstOutput.TryGetProperty("content", out var contentArray) || contentArray.GetArrayLength() == 0)
                throw new Exception("AI response missing 'content'");

            var firstContent = contentArray[0];

            if (!firstContent.TryGetProperty("text", out var textElement))
                throw new Exception("AI response missing 'text'");

            // 7️⃣ Extract the text
            var text = textElement.GetString();

            return text ?? "";
        }
    }
}

