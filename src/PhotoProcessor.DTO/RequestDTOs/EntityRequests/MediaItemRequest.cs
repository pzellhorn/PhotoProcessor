namespace PhotoProcessor.DTO.RequestDTOs.EntityRequests
{
    public class MediaItemRequest
    {
        public MediaItemRequest() { }

        public MediaItemRequest(Guid? mediaItemId, int mediaType, string uri, string thumbnailUri, string contentHash, double? durationMs)
        {
            MediaItemId = mediaItemId;
            MediaType = mediaType;
            Uri = uri;
            ThumbnailUri = thumbnailUri;
            ContentHash = contentHash;
            DurationMs = durationMs;
        }

        public Guid? MediaItemId { get; set; }

        public int MediaType { get; set; }

        public string Uri { get; set; }

        public string ThumbnailUri { get; set; }

        public string ContentHash { get; set; }

        public double? DurationMs { get; set; }
    }
}
