namespace PhotoProcessor.DTO.ServiceDTOs
{ 
    public class SubmitFingerprintsRequest
    {
        public Guid JobId { get; set; }
        public List<DetectedFace> Faces { get; set; } = [];
    }
}
