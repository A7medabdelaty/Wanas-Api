
namespace Wanas.Application.Interfaces.AI
{
    public interface IAIProvider
    {
        Task<string> GenerateTextAsync(string prompt);
    }
}
