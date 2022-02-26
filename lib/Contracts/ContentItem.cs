namespace SDDev.Net.ContentRepository.Contracts
{
    public class ContentItem : IContentItem
    {
        public string FileName { get; set; }
        public string StoragePath { get; set; }
        public byte[] Data { get; set; }
        public string Description { get; set; }
        public string MimeType { get; set; }
        public bool IsPublic { get; set; } = false;
        public string DisplayName { get; set; }
    }
}
