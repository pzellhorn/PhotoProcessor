using PhotoProcessor.DTO.enums;

namespace PhotoProcessor.DTO.ResponseDTOs.EntityResponses
{
    public class JobResponse
    {
        public JobResponse() { }

        public JobResponse(Guid? jobId, Guid mediaId, JobTypes jobType, JobStatus status)
        {
            JobId = jobId;
            MediaId = mediaId;
            JobType = jobType;
            Status = status;
        }

        public Guid? JobId { get; set; }

        public Guid MediaId { get; set; }

        public JobTypes JobType { get; set; }

        public JobStatus Status { get; set; }
    }
}
