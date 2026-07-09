namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class IdentitySummary
    {
        public Guid TagId { get; set; } 
        public string Label { get; set; } = string.Empty;

        public int FaceCount { get; set; }
    }
}
