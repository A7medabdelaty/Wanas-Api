using Wanas.Application.DTOs.Chatbot;

namespace Wanas.Application.Interfaces.AI
{
    public interface IChatbotService
    {
        Task<string> SendMessageAsync(ChatbotRequestDto message);
    }
}
