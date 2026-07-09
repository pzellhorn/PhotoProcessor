namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class FingerprintSummary
    {
        public Guid FingerprintId { get; set; }
        public Guid MediaId { get; set; }
        public Guid? TagId { get; set; }

        public double? DetectionScore { get; set; }

        public double? BoundingX { get; set; }
        public double? BoundingY { get; set; }
        public double? BoundingWidth { get; set; }
        public double? BoundingHeight { get; set; }
    }
}
