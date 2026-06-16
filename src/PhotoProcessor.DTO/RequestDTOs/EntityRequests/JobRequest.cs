using PhotoProcessor.DTO.enums;

namespace PhotoProcessor.DTO.RequestDTOs.EntityRequests
{
    public class JobRequest
    {
        public JobRequest() { }

        public JobRequest(Guid? jobId, Guid mediaId, JobTypes jobType, JobStatus status)
        {
            JobId = jobId;
            MediaId = mediaId;
            JobType = jobType;
            Status = status;
        }

        public Guid? JobId { get; set; }

        public Guid MediaId { get; set; }

        public JobTypes JobType { get; set; } = JobTypes.None;

        public JobStatus Status { get; set; } = JobStatus.None;
    }
}
