using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Wanas.Application.Interfaces;
using Wanas.Application.Settings;

namespace Wanas.Application.Services
{
    public class SecureFileService : ISecureFileService
    {
        private readonly string _basePath;
        private readonly VerificationSettings _settings;
        private readonly byte[] _encryptionKey;

        public SecureFileService(IOptions<VerificationSettings> settings)
        {
            _settings = settings.Value;
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "id-verification");

            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);

            // Derive encryption key from configured key
            _encryptionKey = DeriveKey(_settings.EncryptionKey);
        }

        public async Task<(string encryptedPath, string fileHash)> SaveVerificationDocumentAsync(
            IFormFile file, string userId, string documentType)
        {
            // Validate file
            if (!await ValidateFileAsync(file))
                throw new InvalidOperationException("Invalid file");

            // Calculate file hash before encryption
            var fileHash = await CalculateFileHashAsync(file);

            // Create user-specific directory
            var userDirectory = Path.Combine(_basePath, userId);
            if (!Directory.Exists(userDirectory))
                Directory.CreateDirectory(userDirectory);

            // Generate secure filename
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{documentType}_{timestamp}{extension}";
            var filePath = Path.Combine(userDirectory, fileName);

            // Read file content
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileContent = memoryStream.ToArray();

            // Encrypt and save
            var encryptedContent = EncryptFile(fileContent);
            await File.WriteAllBytesAsync(filePath, encryptedContent);

            // Return encrypted path (relative to base)
            var relativePath = Path.GetRelativePath(_basePath, filePath);
            var encryptedPath = EncryptString(relativePath);

            return (encryptedPath, fileHash);
        }

        public async Task<byte[]> GetVerificationDocumentAsync(string encryptedPath)
        {
            try
            {
                var relativePath = DecryptString(encryptedPath);
                var filePath = Path.Combine(_basePath, relativePath);

                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Verification document not found");

                // Read encrypted file
                var encryptedContent = await File.ReadAllBytesAsync(filePath);

                // Decrypt and return
                return DecryptFile(encryptedContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve verification document", ex);
            }
        }

        public async Task<bool> DeleteVerificationDocumentAsync(string encryptedPath)
        {
            try
            {
                var relativePath = DecryptString(encryptedPath);
                var filePath = Path.Combine(_basePath, relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);

                    // Try to delete user directory if empty
                    var directory = Path.GetDirectoryName(filePath);
                    if (directory != null && Directory.Exists(directory) &&
              !Directory.EnumerateFileSystemEntries(directory).Any())
                    {
                        Directory.Delete(directory);
                    }

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public Task<bool> ValidateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Task.FromResult(false);

            // Check file size
            var maxSizeInBytes = _settings.MaxFileSizeInMB * 1024 * 1024;
            if (file.Length > maxSizeInBytes)
                return Task.FromResult(false);

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_settings.AllowedFileExtensions.Contains(extension))
                return Task.FromResult(false);

            // Validate actual file content (Magic Numbers)
            return ValidateFileMagicNumbersAsync(file, extension);
        }

        private async Task<bool> ValidateFileMagicNumbersAsync(IFormFile file, string extension)
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[8];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            stream.Position = 0;

            return extension switch
            {
                ".jpg" or ".jpeg" => ValidateJpeg(buffer),
                ".png" => ValidatePng(buffer),
                ".pdf" => ValidatePdf(buffer),
                _ => false
            };
        }

        private bool ValidateJpeg(byte[] buffer)
        {
            return buffer.Length >= 3 &&
                buffer[0] == 0xFF &&
                buffer[1] == 0xD8 &&
                buffer[2] == 0xFF;
        }

        private bool ValidatePng(byte[] buffer)
        {
            return buffer.Length >= 8 &&
                buffer[0] == 0x89 &&
                buffer[1] == 0x50 &&
                buffer[2] == 0x4E &&
                buffer[3] == 0x47 &&
                buffer[4] == 0x0D &&
                buffer[5] == 0x0A &&
                buffer[6] == 0x1A &&
                buffer[7] == 0x0A;
        }

        private bool ValidatePdf(byte[] buffer)
        {
            return buffer.Length >= 4 &&
                buffer[0] == 0x25 &&
                buffer[1] == 0x50 &&
                buffer[2] == 0x44 &&
                buffer[3] == 0x46;
        }

        public async Task<string> CalculateFileHashAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return Convert.ToBase64String(hashBytes);
        }

        #region Encryption Helpers

        private byte[] EncryptFile(byte[] plainBytes)
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var memoryStream = new MemoryStream();

            // Write IV first
            memoryStream.Write(aes.IV, 0, aes.IV.Length);

            // Encrypt content
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                cryptoStream.FlushFinalBlock();
            }

            return memoryStream.ToArray();
        }

        private byte[] DecryptFile(byte[] encryptedBytes)
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;

            // Extract IV
            var iv = new byte[aes.IV.Length];
            Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length);
                cryptoStream.FlushFinalBlock();
            }

            return memoryStream.ToArray();
        }

        private string EncryptString(string plainText)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = EncryptFile(plainBytes);
            return Convert.ToBase64String(encryptedBytes);
        }

        private string DecryptString(string encryptedText)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = DecryptFile(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private byte[] DeriveKey(string password)
        {
            // Load salt from settings (configurable via .env)
            var salt = Encoding.UTF8.GetBytes(_settings.EncryptionSalt);
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            return deriveBytes.GetBytes(32); // 256-bit key
        }

        #endregion
    }
}
