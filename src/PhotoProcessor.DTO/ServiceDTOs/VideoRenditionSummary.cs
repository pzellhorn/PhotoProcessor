namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class VideoRenditionSummary
    {
        public string Format { get; set; } = string.Empty;
        public string AssetPath { get; set; } = string.Empty;

        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Bitrate { get; set; }
    }
}
