using Microsoft.AspNetCore.Http;

namespace Wanas.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file); 
        Task<bool> DeleteFileAsync(string url);
    }
}
