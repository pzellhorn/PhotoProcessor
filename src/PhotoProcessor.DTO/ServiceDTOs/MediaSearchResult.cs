using PhotoProcessor.DTO.enums;

namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class MediaSearchResult
    {
        public Guid MediaId { get; set; }
        public MediaItemType MediaType { get; set; }
        public double? DurationMs { get; set; }

        public Guid? ParentMediaId { get; set; }
        public double? TimestampMs { get; set; }

        public double Distance { get; set; }
    }
}
