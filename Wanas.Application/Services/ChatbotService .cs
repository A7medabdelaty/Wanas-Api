
using Wanas.Application.DTOs.Chatbot;
using Wanas.Application.Interfaces.AI;

namespace Wanas.Application.Services
{
    public class ChatbotService :IChatbotService
    {
        private readonly IAIProvider _openAIProvider;

        public ChatbotService(IAIProvider openAIProvider)
        {
            _openAIProvider = openAIProvider;
        }
        public async Task<string> SendMessageAsync(ChatbotRequestDto request)
        {
            var aiResult = await _openAIProvider.GenerateTextAsync(request.Message);
            return aiResult;

            //return new ChatbotResponseDto
            //{
            //    Reply = aiResult
            //};
        }
        }
    
}
