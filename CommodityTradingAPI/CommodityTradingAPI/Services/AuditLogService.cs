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

        public AuditLogService(IConfiguration configuration, BlobServiceClient? blobServiceClient = null)
        {
            string storageConnectionString = configuration["ConnectionStrings:AzureBlobStorage"];
            _containerName = configuration["LogStorageName"];

            _blobServiceClient = blobServiceClient ?? new BlobServiceClient(storageConnectionString);
        }

        public async Task LogChangeAsync(AuditLog auditLog)
        {
            try
            {
                string fileName = $"{auditLog.Timestamp:yyyy_MM_dd}/{auditLog.EntityName}_{auditLog.Timestamp:yyyyMMdd_HHmmss}.log";

                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                if (!await containerClient.ExistsAsync())
                {
                    await containerClient.CreateAsync();
                }

                string logContent = JsonSerializer.Serialize(auditLog, new JsonSerializerOptions { WriteIndented = true });

                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(logContent)))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
            }
            catch (Exception ex)
            {
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
