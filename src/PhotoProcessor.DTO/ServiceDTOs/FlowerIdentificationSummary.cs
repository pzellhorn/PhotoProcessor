namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class FlowerIdentificationSummary
    {
        public Guid MediaId { get; set; }
        public bool IsFlower { get; set; }
        public double? FlowerScore { get; set; }
        public List<FlowerPredictionDto> Predictions { get; set; } = new();
    }
}
