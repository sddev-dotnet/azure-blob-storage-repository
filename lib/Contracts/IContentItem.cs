namespace SDDev.Net.ContentRepository.Contracts
{
    public interface IContentItem
    {
        /// <summary>
        /// This is the friendly display name for the file
        /// when we save it, the file name is converted to a Guid to
        /// ensure we never end up with conflicting file names
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// This is the actual file name in the storage location (the guid we create)
        /// </summary>
        string FileName { get; set; }
        
        /// <summary>
        /// This is the full path to the item and is used to retrieve the content item 
        /// </summary>
        string StoragePath { get; set; }
        
        /// <summary>
        /// The byte data for the object
        /// </summary>
        byte[] Data { get; set; }

        /// <summary>
        /// An optional description
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// The string mime type for the item (use the FileHelpers class to find this)
        /// </summary>
        string MimeType { get; set; }

        /// <summary>
        /// Whether or not the uploaded item should be publicly visible
        /// </summary>
        bool IsPublic { get; set; }
    }
}
