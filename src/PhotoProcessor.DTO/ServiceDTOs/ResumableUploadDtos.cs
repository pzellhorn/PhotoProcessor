using PhotoProcessor.DTO.enums;

namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class CreateUploadRequest
    {
        public string FileName { get; set; } = string.Empty;
        public long TotalBytes { get; set; }

        /// <summary>Optional client-computed SHA-256. When it matches existing media the upload is skipped entirely.</summary>
        public string? ContentHash { get; set; }
    }

    public class CreateUploadResponse
    {
        public Guid UploadId { get; set; }
        public int ChunkSize { get; set; }
        public long ReceivedBytes { get; set; }

        public bool Duplicate { get; set; }
        public Guid? MediaId { get; set; }
    }

    public class UploadStatusResponse
    {
        public Guid UploadId { get; set; }
        public long TotalBytes { get; set; }
        public long ReceivedBytes { get; set; }
        public int ChunkSize { get; set; }

        public bool Complete { get; set; }
        public bool Duplicate { get; set; }
        public Guid? MediaId { get; set; }
        public MediaItemType MediaType { get; set; }
    }
}
