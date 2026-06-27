namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class MarkJobFailedRequest(Guid jobId, string? error)
    {
        public Guid JobId { get; set; } = jobId;
        public string? Error { get; set; } = error;
    }
}
