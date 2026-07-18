namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class SubmitImageEmbeddingRequest
    {
        public Guid JobId { get; set; }
        public float[] Embedding { get; set; } = [];
    }
}
