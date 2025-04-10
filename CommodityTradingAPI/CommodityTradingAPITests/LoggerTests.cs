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

            _configurationMock
                .Setup(c => c["ConnectionStrings:AzureBlobStorage"])
                .Returns("UseDevelopmentStorage=true");

            _configurationMock
                .Setup(c => c["LogStorageName"])
                .Returns("auditlogs");

            _blobServiceClientMock
                .Setup(b => b.GetBlobContainerClient("auditlogs"))
                .Returns(_blobContainerClientMock.Object);

            _blobContainerClientMock
                .Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, null));

            _blobContainerClientMock
                .Setup(c => c.CreateAsync(It.IsAny<PublicAccessType>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Response<BlobContainerInfo>)null!);

            _blobContainerClientMock
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClientMock.Object);

            _blobClientMock
                .Setup(c => c.UploadAsync(
                    It.IsAny<Stream>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Response<BlobContentInfo>)null!);

            _auditLogService = new AuditLogService(_configurationMock.Object, _blobServiceClientMock.Object);
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
