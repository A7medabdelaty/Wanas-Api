using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wanas.Application.Interfaces;
using Wanas.Application.Settings;
using Wanas.Domain.Repositories;

namespace Wanas.Infrastructure.Services
{
    public class DocumentCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DocumentCleanupService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISecureFileService _secureFileService;
        private readonly VerificationSettings _settings;

        public DocumentCleanupService(
            IServiceProvider serviceProvider,
            ILogger<DocumentCleanupService> logger,
            IOptions<VerificationSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Document Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_settings.EnableAutoDelete)
                    {
                        await CleanupDocumentsAsync();
                    }

                    // Run cleanup once per day at 2 AM
                    var now = DateTime.UtcNow;
                    var next2AM = now.Date.AddDays(1).AddHours(2);
                    var delay = next2AM - now;

                    _logger.LogInformation($"Next cleanup scheduled at {next2AM:yyyy-MM-dd HH:mm:ss} UTC");

                    await Task.Delay(delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Document Cleanup Service");
                    // Wait 1 hour before retrying on error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task CleanupDocumentsAsync()
        {
            var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _secureFileService = scope.ServiceProvider.GetRequiredService<ISecureFileService>();
            try
            {
                _logger.LogInformation("Starting document cleanup...");

                var documentsToDelete = await _unitOfWork.VerificationDocuments.GetDocumentsForDeletionAsync();

                if (!documentsToDelete.Any())
                {
                    _logger.LogInformation("No documents to delete");
                    return;
                }

                _logger.LogInformation($"Found {documentsToDelete.Count()} documents to delete");

                var deletedCount = 0;
                var failedCount = 0;

                foreach (var document in documentsToDelete)
                {
                    try
                    {
                        // Delete physical file
                        var fileDeleted = await _secureFileService.DeleteVerificationDocumentAsync(document.EncryptedFilePath);

                        if (fileDeleted)
                        {
                            // Mark as deleted in database
                            document.IsDeleted = true;
                            _unitOfWork.VerificationDocuments.Update(document);
                            deletedCount++;

                            _logger.LogInformation($"Deleted document {document.Id} for user {document.UserId}");
                        }
                        else
                        {
                            failedCount++;
                            _logger.LogWarning($"Failed to delete file for document {document.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        _logger.LogError(ex, $"Error deleting document {document.Id}");
                    }
                }

                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Document cleanup completed. Deleted: {deletedCount}, Failed: {failedCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during document cleanup");
            }
        }
    }
}
