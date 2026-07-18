namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class SubmitKeyframesRequest
    {
        public Guid JobId { get; set; }
        public List<KeyframeDto> Frames { get; set; } = [];
    }

    public class KeyframeDto
    {
        public double TimestampMs { get; set; }
        public string StoragePath { get; set; } = string.Empty;
    }
}
