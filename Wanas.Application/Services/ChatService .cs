
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
         public async Task<string> SendMessageAsync(string message)
        {
            return await _openAIProvider.GenerateTextAsync(message);
        }

    }
    
}
