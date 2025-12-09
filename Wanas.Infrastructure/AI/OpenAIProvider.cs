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
        private readonly string _baseUrl;

        public OpenAIProvider(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            // Read from OpenAI section (matches .env OpenAI__ApiKey format)
            _apiKey = config["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI:ApiKey is missing.");


            _baseUrl = config["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            try
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
                var response = await _httpClient.PostAsync($"{_baseUrl}/responses", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"OpenAI API Error: {response.StatusCode} - {errorContent}");
                    return $"I apologize, but I'm unable to process your request at the moment. Please try again later.";
                }

                var resultJson = await response.Content.ReadAsStringAsync();

                //  Parse JSON
                using var doc = JsonDocument.Parse(resultJson);
                var root = doc.RootElement;

                
                if (!root.TryGetProperty("output", out var outputArray) || outputArray.GetArrayLength() == 0)
                {
                    Console.WriteLine("AI response missing 'output'");
                    return "I apologize, but I couldn't generate a response. Please try again.";
                }

                var firstOutput = outputArray[0];

                if (!firstOutput.TryGetProperty("content", out var contentArray) || contentArray.GetArrayLength() == 0)
                {
                    Console.WriteLine("AI response missing 'content'");
                    return "I apologize, but I couldn't generate a response. Please try again.";
                }

                var firstContent = contentArray[0];

                if (!firstContent.TryGetProperty("text", out var textElement))
                {
                    Console.WriteLine("AI response missing 'text'");
                    return "I apologize, but I couldn't generate a response. Please try again.";
                }

                var text = textElement.GetString();

                return text ?? "I apologize, but I couldn't generate a response. Please try again.";
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error calling OpenAI: {ex.Message}");
                return "I'm currently experiencing connectivity issues. Please try again later.";
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing OpenAI response: {ex.Message}");
                return "I received an unexpected response. Please try again.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in OpenAI provider: {ex.Message}");
                return "I encountered an unexpected error. Please try again later.";
            }
        }
    }
}

