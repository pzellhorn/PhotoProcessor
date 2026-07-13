namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class SubmitVideoRequest
    {
        public Guid JobId { get; set; }

        public double DurationMs { get; set; }

        public string PosterPath { get; set; } = string.Empty;

        public List<VideoRenditionDto> Renditions { get; set; } = [];
    }

    public class VideoRenditionDto
    {
        public string Format { get; set; } = string.Empty;
        public string EntryPath { get; set; } = string.Empty;

        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Bitrate { get; set; }
    }
}
