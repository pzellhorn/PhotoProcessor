namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class MarkJobRunningRequest(Guid jobId)
    {
        public Guid JobId { get; set; } = jobId;
    }
}
