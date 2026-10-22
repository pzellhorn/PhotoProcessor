namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class SubmitFlowerIdentificationRequest
    {
        public Guid JobId { get; set; }
        public float[] Embedding { get; set; } = [];
        public List<FlowerPredictionDto> Predictions { get; set; } = new();
    }
}
