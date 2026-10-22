namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class FlowerPredictionDto
    {
        public string ScientificName { get; set; } = string.Empty;
        public string? CommonName { get; set; }
        public string Family { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
