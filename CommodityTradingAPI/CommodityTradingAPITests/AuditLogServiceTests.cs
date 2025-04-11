using NUnit.Framework;
using Moq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using CommodityTradingAPI.Models;
using CommodityTradingAPI.Services;
using Azure;

namespace CommodityTradingAPITests
{
    [TestFixture]
    public class AuditLogServiceTests
    {
        private Mock<IConfiguration> _configurationMock;
        private Mock<IConfigurationSection> _connStringSectionMock;
        private Mock<IConfigurationSection> _containerNameSectionMock;
        private Mock<BlobServiceClient> _blobServiceClientMock;
        private Mock<BlobContainerClient> _blobContainerClientMock;
        private Mock<BlobClient> _blobClientMock;

        private AuditLogService _auditLogService;

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _blobServiceClientMock = new Mock<BlobServiceClient>();
            _blobContainerClientMock = new Mock<BlobContainerClient>();
            _blobClientMock = new Mock<BlobClient>();

            // Mock the configuration to return the expected connection string and log storage name
            _configurationMock
                .Setup(c => c["ConnectionStrings:AzureBlobStorage"]) // This is the key for connection string
                .Returns("UseDevelopmentStorage=true"); // Simulating a valid connection string

            _configurationMock
                .Setup(c => c["LogStorageName"]) // Mock the log storage container name
                .Returns("auditlogs");

            // Mock the BlobServiceClient to return BlobContainerClient
            _blobServiceClientMock
                .Setup(b => b.GetBlobContainerClient("auditlogs"))
                .Returns(_blobContainerClientMock.Object);

            // Mock BlobContainerClient to return that the container doesn't exist yet
            _blobContainerClientMock
                .Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, null));

            // Mock creating the container
            _blobContainerClientMock
                .Setup(c => c.CreateAsync(It.IsAny<PublicAccessType>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Response<BlobContainerInfo>)null!);

            // Mock getting a blob client
            _blobContainerClientMock
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClientMock.Object);

            // Mock the blob upload
            _blobClientMock
                .Setup(c => c.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Response<BlobContentInfo>)null!);

            // Now instantiate the service with the mock configuration
            _auditLogService = new AuditLogService(_configurationMock.Object);
        }


        [Test]
        public async Task LogChangeAsync_ValidAuditLog_UploadsToBlobStorage()
        {
            var log = new AuditLog
            {
                EntityName = "User",
                Action = "Create",
                ChangedBy = "admin",
                Timestamp = DateTime.UtcNow,
                Details = "Created user JohnDoe"
            };

            await _auditLogService.LogChangeAsync(log);

            _blobContainerClientMock.Verify(c => c.CreateAsync(
                It.IsAny<PublicAccessType>(),
                null,
                null,
                It.IsAny<CancellationToken>()),
                Times.Once);

            _blobClientMock.Verify(b => b.UploadAsync(
                It.IsAny<Stream>(),
                true,
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task LogChangeAsync_WhenExceptionThrown_DoesntCrash()
        {
            // Fake exception
            _blobContainerClientMock
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Throws(new Exception("Simulated failure"));

            var log = new AuditLog
            {
                EntityName = "User",
                Action = "Delete",
                ChangedBy = "admin",
                Timestamp = DateTime.UtcNow,
                Details = "Deleted user JaneDoe"
            };

            Assert.DoesNotThrowAsync(() => _auditLogService.LogChangeAsync(log));
        }

        // Helper to inject mocks
        private class TestableAuditLogService : AuditLogService
        {
            private readonly BlobServiceClient _mockBlobServiceClient;

            public TestableAuditLogService(IConfiguration config, BlobServiceClient mockClient)
                : base(config)
            {
                _mockBlobServiceClient = mockClient;
            }
        }
    }
}
