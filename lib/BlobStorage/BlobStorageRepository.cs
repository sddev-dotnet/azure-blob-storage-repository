using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SDDev.Net.ContentRepository.Contracts;
using SDDev.Net.ContentRepository.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDDev.Net.ContentRepository.BlobStorage
{
    public class BlobStorageContentRepository : IContentRepository
    {
        string _blobName;
        protected BlobServiceClient Client;
        protected BlobStorageConfiguration Configuration;
        protected BlobContainerClient Container;
        protected ILogger<BlobStorageContentRepository> Logger;

        public string BlobName
        {
            get => _blobName;
            set
            {
                Container = Client.GetBlobContainerClient(value.ToLower());

                if (Configuration.CreateIfNotExists)
                {
                    Container.CreateIfNotExistsAsync().GetAwaiter().GetResult();
                }

                _blobName = value.ToLower();
            }
        }

        #region Constructor
        public BlobStorageContentRepository(BlobServiceClient client, IOptions<BlobStorageConfiguration> config, ILogger<BlobStorageContentRepository> logger)
        {
            Configuration = config.Value;
            Client = client;
            BlobName = config.Value.DefaultBlob;
            Logger = logger;
        }
        #endregion

        #region IContentRepository Methods
        public async Task<IStorageModel> Create(IContentItem item)
        {
            try
            {
                item.FileName = RemoveContainerName(item.StoragePath);
                var blob = Container.GetBlobClient(item.FileName);

                using (var stream = new MemoryStream(item.Data))
                {
                    await blob.UploadAsync(stream, true);
                }

                var (fileType, mime, extension) = FileHelper.GetFileType(item.StoragePath);

                var storageItem = new StorageModel
                {
                    Location = StorageLocation.BLOB,
                    StoragePath = item.StoragePath,
                    FileSize = item.Data.LongLength,
                    FileType = new FileType()
                    {
                        MimeType = mime,
                        Name = fileType.ToString()
                    },
                    FileName = GetFileName(item.StoragePath)
                };

                return storageItem;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error:{ex.Message}, Stacktrace: {ex.StackTrace}");
            }

            return null;
        }

        public async Task<IContentItem> Get(IStorageModel model)
        {
            try
            {
                model.StoragePath = RemoveContainerName(model.StoragePath);
                var blob = Container.GetBlobClient(model.StoragePath);
                var exists = await blob.ExistsAsync().ConfigureAwait(false);

                if (exists)
                {
                    using var stream = new MemoryStream();
                    var bytes = await blob.DownloadToAsync(stream).ConfigureAwait(false);
                    var contentItem = new ContentItem
                    {
                        FileName = $"{BlobName}/{model.StoragePath}",
                        Data = stream.ToArray(),
                        MimeType = model.FileType?.MimeType ?? string.Empty
                    };

                    return contentItem;
                }

                Logger.LogInformation($"{model.StoragePath} doesn't exists in BLOB: {BlobName}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error:{ex.Message}, Stacktrace: {ex.StackTrace}");
            }

            return null;
        }

        public async Task Delete(IStorageModel item)
        {
            var path = RemoveContainerName(item.StoragePath);
            var blob = Container.GetBlobClient(path);

            await blob.DeleteIfExistsAsync();
        }
        #endregion

        #region Private Methods
        string RemoveContainerName(string path)
        {
            if (!path.StartsWith($"{BlobName}/"))
            {
                return path;
            }

            var stringLength = $"{BlobName}/".Length;
            return path.Substring(stringLength);
        }

        

        string GetFileName(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return path.Split('/').Last();
            }

            return string.Empty;
        }
        #endregion
    }
}
