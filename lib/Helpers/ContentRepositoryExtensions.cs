using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDDev.Net.ContentRepository.BlobStorage;
using SDDev.Net.ContentRepository.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDDev.Net.ContentRepository.Helpers
{
    public static class BlobStorageExtensions
    {
        public static void UseBlobStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection("BlobStorage");
            var config = configSection.Get<BlobStorageConfiguration>();

            services.Configure<BlobStorageConfiguration>(configSection);

            services.AddSingleton(x =>
            {
                if (string.IsNullOrEmpty(config.StorageAccountConnectionString))
                {
                    throw new ArgumentNullException("Invalid Blob Storage Connection String.");
                }

                var client = new BlobServiceClient(config.StorageAccountConnectionString);
                return client;
            });

            services.Configure<BlobStorageConfiguration>(configSection);

            services.AddScoped(typeof(IContentRepository), typeof(BlobStorageContentRepository));
        }
    }
}
