using PhotoProcessor.DTO.enums;

namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class MediaLibraryPage
    {
        public List<MediaLibraryItem> Items { get; set; } = [];
        public int TotalCount { get; set; }
    }

    public class MediaLibraryItem
    {
        public Guid MediaItemId { get; set; }
        public MediaItemType MediaType { get; set; }
        public double? DurationMs { get; set; }
    }
}
