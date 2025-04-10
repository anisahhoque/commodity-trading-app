using Azure.Storage.Blobs;
using CommodityTradingAPI.Models;
using System.Text.Json;
using CommodityTradingAPI.Services;

namespace CommodityTradingAPI.Services
{
    public class AuditLogService : ILogger
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly string _storageConnectionString;

        public AuditLogService(IConfiguration configuration)
        {
            _storageConnectionString = configuration.GetConnectionString("AzureBlobStorage");
            _containerName = configuration["LogStorageName"];
            _blobServiceClient = new BlobServiceClient(_storageConnectionString);
        }

        public async Task LogChangeAsync(AuditLog auditLog)
        {
            try
            {
                // Generate the log file name based on the entity name and timestamp
                string fileName = $"{auditLog.Timestamp:yyyy_MM_dd}/{auditLog.EntityName}_{auditLog.Timestamp:yyyyMMdd_HHmmss}.log";

                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                if (!await containerClient.ExistsAsync())
                {
                    await containerClient.CreateAsync();
                }

                string logContent = JsonSerializer.Serialize(auditLog, new JsonSerializerOptions { WriteIndented = true });

                // Upload the log
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(logContent)))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
            }
            catch (Exception ex)
            {
                // Output any logging errors to terminal
                Console.WriteLine($"Error logging change: {ex.Message}");
            }
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
    }

}
