using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDDev.Net.ContentRepository.Helpers
{
    /// <summary>
    /// Used to assist with converting extensions to known file types and vice versa
    /// </summary>
    public static class FileHelper
    {
        private static Dictionary<string, SupportedFileTypes> FileTypeMap = new Dictionary<string, SupportedFileTypes>()
        {
            {"image/gif", SupportedFileTypes.GIF },
            {"image/png", SupportedFileTypes.PNG },
            {"image/jgp", SupportedFileTypes.JPG },
            {"image/jpeg", SupportedFileTypes.JPG },

            {"application/vnd.openxmlformats-officedocument.wordprocessingml.document", SupportedFileTypes.DOCX },
            {"application/msword", SupportedFileTypes.DOCX },

            {"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", SupportedFileTypes.XLSX },
            {"application/vnd.ms-excel", SupportedFileTypes.XLSX },

            {"text/csv", SupportedFileTypes.CSV },
            {"text/xml", SupportedFileTypes.XML },
            {"application/xml", SupportedFileTypes.XML }
        };

        // Used to map an extension to a mime type
        private static Dictionary<string, string> ExtensionMap = new Dictionary<string, string>()
        {
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".csv", "text/csv" },
            { ".xml", "text/xml" },
            { ".jpg", "image/jgp" },
            { ".jpeg", "image/jgp" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".pdf", "application/pdf"}
        };

        /// <summary>
        /// Returns the Parsed Enum SupportedFileType, the mime type, and the extension of the file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static (SupportedFileTypes, string, string) GetFileType(string fileName)
        {
            var extensionIndex = fileName.LastIndexOf(".");
            var extension = fileName.Substring(extensionIndex);

            var mimeType = ExtensionMap.FirstOrDefault(f => f.Key == extension).Value;

            var fileType = FileTypeMap.FirstOrDefault(f => f.Key == mimeType).Value;


            return (fileType, mimeType, extension);
        }

        public static IList<string> GetMimeTypes(SupportedFileTypes fileType)
        {
            return FileTypeMap.Where(x => x.Value == fileType).Select(x => x.Key).ToList();
        }
    }
}
