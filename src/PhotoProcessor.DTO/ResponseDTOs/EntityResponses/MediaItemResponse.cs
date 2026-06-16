using PhotoProcessor.DTO.enums;

namespace PhotoProcessor.DTO.ResponseDTOs.EntityResponses
{
    public class MediaItemResponse
    {
        public MediaItemResponse() { }

        public MediaItemResponse(Guid? mediaItemId, MediaItemType mediaType, string uri, string thumbnailUri, string contentHash, double? durationMs)
        {
            MediaItemId = mediaItemId;
            MediaType = mediaType;
            Uri = uri;
            ThumbnailUri = thumbnailUri;
            ContentHash = contentHash;
            DurationMs = durationMs;
        }

        public Guid? MediaItemId { get; set; }

        public MediaItemType MediaType { get; set; }

        public string Uri { get; set; }

        public string ThumbnailUri { get; set; }

        public string ContentHash { get; set; }

        public double? DurationMs { get; set; }
    }
}
