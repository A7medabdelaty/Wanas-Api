namespace Wanas.Application.Interfaces.AI
{
    public interface IChatbotService
    {
        Task<string> SendMessageAsync(string message);
    }
}
