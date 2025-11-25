using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Wanas.Application.Interfaces;

public class FileService : IFileService
{
    private readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

    private readonly Cloudinary _cloudinary;

    public FileService(Cloudinary cloudinary)
    {
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);

        _cloudinary = cloudinary;
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
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new Exception("No file provided");

        // Validate allowed extensions
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLower();

        if (!allowed.Contains(ext))
            throw new Exception("Invalid file type");

        // Convert file → stream
        await using var stream = file.OpenReadStream();

        // Prepare Cloudinary upload parameters
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "wanas-web/users" // folder name in Cloudinary
        };

        // Upload to Cloudinary
        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception("Cloudinary upload failed");

        // Return the URL
        return uploadResult.SecureUrl.AbsoluteUri;
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
