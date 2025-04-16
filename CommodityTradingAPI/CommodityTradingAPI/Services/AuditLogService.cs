using Azure.Storage.Blobs;
using CommodityTradingAPI.Models;
using System.Text.Json;
using CommodityTradingAPI.Services;

namespace CommodityTradingAPI.Services
{
    public class AuditLogService : ILogger
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;

        public AuditLogService(IConfiguration configuration)
        {
            var storageConnectionString = configuration.GetConnectionString("AzureBlobStorage");
            var containerName = configuration["LogStorageName"];

            _blobServiceClient = new BlobServiceClient(storageConnectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists at startup
            _containerClient.CreateIfNotExists();
        }

        public async Task LogChangeAsync(ILog auditLog)
        {
            try
            {
                string fileName = $"{auditLog.Timestamp:yyyy_MM_dd}/{auditLog.EntityName}_{auditLog.Timestamp:yyyyMMdd_HHmmss}.log";
                string logContent = JsonSerializer.Serialize(auditLog, new JsonSerializerOptions { WriteIndented = true });

                BlobClient blobClient = _containerClient.GetBlobClient(fileName);
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(logContent));
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging change: {ex.Message}");
            }
        }

        public async Task CreateNewLogAsync(string entityName, string action, string changedBy, string details, bool sus)
        {
            var auditLog = new AuditLog
            {
                EntityName = entityName,
                Action = action,
                ChangedBy = changedBy,
                Timestamp = DateTime.UtcNow,
                Details = details,
                Suspicious = sus
            };

            await LogChangeAsync(auditLog);
        }
    }
}



namespace CommodityTradingAPI.Models
{
    public class AuditLog : ILog
    {
        public string EntityName { get; set; } // What was changed
        public string Action { get; set; } // How it was changed
        public string ChangedBy { get; set; } // Who changed it
        public DateTime Timestamp { get; set; } // When did it get changed
        public string Details { get; set; } // Extra info
        public bool Suspicious { get; set; } // Is sus?
    }

}
