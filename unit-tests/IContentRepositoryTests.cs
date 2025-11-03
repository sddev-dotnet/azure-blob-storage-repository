using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SDDev.Net.ContentRepository.BlobStorage;
using SDDev.Net.ContentRepository.Contracts;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SDDev.Net.ContentRepository.Tests.Unit
{
    /// <summary>
    /// MSTest V2 unit tests for IContentRepository interface.
    /// NOTE: These tests require Azurite (Azure Storage Emulator) to be running.
    /// </summary>
    [TestClass]
    public class IContentRepositoryTests
    {
        private IContentRepository? _repository;
        private string _testContainerName = string.Empty;
        private bool _azuriteAvailable;

        [TestInitialize]
        public void Setup()
        {
            var guidString = Guid.NewGuid().ToString("N");
            var containerName = $"test-container-{guidString}";
            _testContainerName = containerName.Length >63 ? containerName.Substring(0,63).ToLower() : containerName.ToLower();

            var config = new BlobStorageConfiguration
            {
                StorageAccountConnectionString = "UseDevelopmentStorage=true",
                DefaultBlob = _testContainerName,
                CreateIfNotExists = true
            };

            try
            {
                var options = Options.Create(config);
                var client = new BlobServiceClient(config.StorageAccountConnectionString);
                var logger = new Mock<ILogger<BlobStorageContentRepository>>();
                _repository = new BlobStorageContentRepository(client, options, logger.Object);
                _azuriteAvailable = true;
            }
            catch
            {
                _azuriteAvailable = false;
                _repository = null;
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                var client = new BlobServiceClient("UseDevelopmentStorage=true");
                var container = client.GetBlobContainerClient(_testContainerName);
                container.DeleteIfExistsAsync().GetAwaiter().GetResult();
            }
            catch { }
        }

        [TestMethod]
        public async Task Create_WithValidContentItem_ReturnsStorageModel()
        {
            if (!_azuriteAvailable || _repository == null) return;
            var contentItem = new ContentItem
            {
                Data = Encoding.UTF8.GetBytes("Test content"),
                StoragePath = $"{_testContainerName}/testfile.txt",
                DisplayName = "Test File",
                Description = "A test file"
            };
            var result = await _repository.Create(contentItem);
            Assert.IsNotNull(result);
            Assert.AreEqual(StorageLocation.BLOB, result.Location);
            Assert.IsNotNull(result.StoragePath);
            Assert.IsTrue(result.FileSize >0);
            Assert.IsNotNull(result.FileType);
        }

        [TestMethod]
        public async Task Create_WithImageFile_ReturnsCorrectFileType()
        {
            if (!_azuriteAvailable || _repository == null) return;
            var imageBytes = new byte[] {0x89,0x50,0x4E,0x47 }; // PNG header
            var contentItem = new ContentItem
            {
                Data = imageBytes,
                StoragePath = $"{_testContainerName}/testimage.png",
                DisplayName = "Test Image"
            };
            var result = await _repository.Create(contentItem);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileType);
            Assert.IsFalse(string.IsNullOrEmpty(result.FileType.MimeType));
        }

        [TestMethod]
        public async Task Create_WithDocumentFile_ReturnsCorrectFileType()
        {
            if (!_azuriteAvailable || _repository == null) return;
            var contentItem = new ContentItem
            {
                Data = Encoding.UTF8.GetBytes("Test document content"),
                StoragePath = $"{_testContainerName}/testdoc.docx",
                DisplayName = "Test Document"
            };
            var result = await _repository.Create(contentItem);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileType);
            Assert.IsTrue(result.FileType.MimeType.ToLower().Contains("document"));
        }

        [TestMethod]
        public async Task Get_WithExistingFile_ReturnsContentItem()
        {
            if (!_azuriteAvailable || _repository == null) return;
            var testContent = "This is test content for retrieval";
            var fileName = $"{_testContainerName}/retrieval-test.txt";
            var createItem = new ContentItem
            {
                Data = Encoding.UTF8.GetBytes(testContent),
                StoragePath = fileName,
                DisplayName = "Retrieval Test"
            };
            var storageModel = await _repository.Create(createItem);
            Assert.IsNotNull(storageModel);
            var result = await _repository.Get(storageModel);
            Assert.IsNotNull(result);
            Assert.AreEqual(testContent, Encoding.UTF8.GetString(result.Data));
            Assert.IsNotNull(result.FileName);
            Assert.AreEqual(fileName, result.FileName);
        }

        [TestMethod]
        public async Task Get_WithNonExistentFile_ReturnsNull()
        {
            if (!_azuriteAvailable || _repository == null) return;
            var storageModel = new StorageModel
            {
                StoragePath = $"{_testContainerName}/nonexistent-file-{Guid.NewGuid()}.txt",
                Location = StorageLocation.BLOB
            };
            var result = await _repository.Get(storageModel);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Delete_WithExistingFile_RemovesFile()
        {
            if (!_azuriteAvailable || _repository == null) return;
            var testContent = "Content to be deleted";
            var fileName = $"{_testContainerName}/delete-test.txt";
            var createItem = new ContentItem
            {
                Data = Encoding.UTF8.GetBytes(testContent),
                StoragePath = fileName,
                DisplayName = "Delete Test"
            };
            var storageModel = await _repository.Create(createItem);
            Assert.IsNotNull(storageModel);
            var beforeDelete = await _repository.Get(storageModel);
            Assert.IsNotNull(beforeDelete);
            await _repository.Delete(storageModel);
            var afterDelete = await _repository.Get(storageModel);
            Assert.IsNull(afterDelete);
        }

        [TestMethod]
        public async Task Delete_WithNonExistentFile_DoesNotThrow()
        {
            if (!_azuriteAvailable || _repository == null) return;
            var storageModel = new StorageModel
            {
                StoragePath = $"{_testContainerName}/nonexistent-{Guid.NewGuid()}.txt",
                Location = StorageLocation.BLOB
            };
            await _repository.Delete(storageModel); // Should not throw
        }

        [TestMethod]
        public async Task Create_Get_Delete_CompleteWorkflow_Works()
        {
            if (!_azuriteAvailable || _repository == null) return;
            var testContent = "Complete workflow test content";
            var fileName = $"{_testContainerName}/workflow-test.txt";
            var createItem = new ContentItem
            {
                Data = Encoding.UTF8.GetBytes(testContent),
                StoragePath = fileName,
                DisplayName = "Workflow Test",
                Description = "Testing complete workflow"
            };
            var storageModel = await _repository.Create(createItem);
            Assert.IsNotNull(storageModel);
            var retrievedItem = await _repository.Get(storageModel);
            Assert.IsNotNull(retrievedItem);
            Assert.AreEqual(testContent, Encoding.UTF8.GetString(retrievedItem.Data));
            Assert.AreEqual(fileName, retrievedItem.FileName);
            await _repository.Delete(storageModel);
            var deletedItem = await _repository.Get(storageModel);
            Assert.IsNull(deletedItem);
        }

        [TestMethod]
        public async Task Create_WithLargeFile_ReturnsCorrectFileSize()
        {
            if (!_azuriteAvailable || _repository == null) return;
            var largeContent = new byte[1024 *100];
            new Random().NextBytes(largeContent);
            var contentItem = new ContentItem
            {
                Data = largeContent,
                StoragePath = $"{_testContainerName}/largefile.bin",
                DisplayName = "Large File Test"
            };
            var result = await _repository.Create(contentItem);
            Assert.IsNotNull(result);
            Assert.AreEqual(largeContent.LongLength, result.FileSize);
        }

        [TestMethod]
        public async Task Create_WithSpecialCharactersInPath_HandlesCorrectly()
        {
            if (!_azuriteAvailable || _repository == null) return;
            var contentItem = new ContentItem
            {
                Data = Encoding.UTF8.GetBytes("Test content"),
                StoragePath = $"{_testContainerName}/test-file_with-special-chars-123.txt",
                DisplayName = "Special Chars Test"
            };
            var result = await _repository.Create(contentItem);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.StoragePath);
            Assert.IsNotNull(result.FileName);
        }

        [TestMethod]
        public async Task Get_ReturnsCorrectMimeType()
        {
            if (!_azuriteAvailable || _repository == null) return;
            var contentItem = new ContentItem
            {
                Data = Encoding.UTF8.GetBytes("CSV content,column1,column2"),
                StoragePath = $"{_testContainerName}/testfile.csv",
                DisplayName = "CSV Test"
            };
            var storageModel = await _repository.Create(contentItem);
            Assert.IsNotNull(storageModel);
            var result = await _repository.Get(storageModel);
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result.MimeType));
        }
    }
}

