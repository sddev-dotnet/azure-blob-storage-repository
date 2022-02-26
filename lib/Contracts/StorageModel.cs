using System;

namespace SDDev.Net.ContentRepository.Contracts
{
    public class StorageModel : IStorageModel
    {
        public string FileName { get; set; }
        public StorageLocation Location { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string StoragePath { get; set; }
        public FileType FileType { get; set; }
        public long FileSize { get; set; }
    }
}
