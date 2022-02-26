using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDDev.Net.ContentRepository.Contracts
{
    public class BlobStorageConfiguration
    {
        public string StorageAccountConnectionString { get; set; }
        public string DefaultBlob { get; set; }
        public bool CreateIfNotExists { get; set; }
    }
}
