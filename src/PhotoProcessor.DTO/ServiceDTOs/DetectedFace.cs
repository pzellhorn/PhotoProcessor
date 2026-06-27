namespace PhotoProcessor.DTO.ServiceDTOs
{ 
    public class DetectedFace
    {
        public float[] Embedding { get; set; } = [];
        public double? DetectionScore { get; set; }
        public double? BoundingX { get; set; }
        public double? BoundingY { get; set; }
        public double? BoundingWidth { get; set; }
        public double? BoundingHeight { get; set; }
    }
}
