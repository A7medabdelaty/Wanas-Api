using Microsoft.AspNetCore.Http;

namespace Wanas.Application.Interfaces
{
    public interface ISecureFileService
    {

        // Securely saves a verification document with encryption
        Task<(string encryptedPath, string fileHash)> SaveVerificationDocumentAsync(IFormFile file, string userId, string documentType);

        // Retrieves and decrypts a verification document
        Task<byte[]> GetVerificationDocumentAsync(string encryptedPath);

        // Securely deletes a verification document
        Task<bool> DeleteVerificationDocumentAsync(string encryptedPath);

        // Validates file before upload
        Task<bool> ValidateFileAsync(IFormFile file);

        // Calculates SHA256 hash of file for integrity verification
        Task<string> CalculateFileHashAsync(IFormFile file);
    }
}
