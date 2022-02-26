using System;

namespace SDDev.Net.ContentRepository.Contracts
{
    public interface IStorageModel
    {
        string FileName { get; set; }

        StorageLocation Location { get; set; }

        DateTime CreatedDateTime { get; set; }

        string StoragePath { get; set; }

        FileType FileType { get; set; }

        long FileSize { get; set; }
    }
}
