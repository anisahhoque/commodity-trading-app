using Azure.Storage.Blobs;
using CommodityTradingAPI.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommodityTradingAPI.Services
{
    public class AuditLogService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "laughingstock-audit-logs";
        private readonly string _storageConnectionString;

        public AuditLogService(string connectionString)
        {
            _storageConnectionString = connectionString;
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

                string logContent = JsonSerializer.Serialize(auditLog);

                // Upload
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(logContent)))
                {
                    await blobClient.UploadAsync(stream);
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
    public class AuditLog
    {
        public string EntityName { get; set; } // What was changed
        public string Action { get; set; } // How it was changed
        public string ChangedBy { get; set; } // Who changed it
        public DateTime Timestamp { get; set; } // When did it get changed
        public string Details { get; set; } // Extra info
    }

}
