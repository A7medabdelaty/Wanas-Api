using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Wanas.Application.Interfaces;

public class FileService : IFileService
{
    private readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

    public FileService()
    {
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(_basePath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Return relative URL
        return $"/uploads/{fileName}";
    }

    public Task<bool> DeleteFileAsync(string url)
    {
        var fileName = Path.GetFileName(url);
        var filePath = Path.Combine(_basePath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
