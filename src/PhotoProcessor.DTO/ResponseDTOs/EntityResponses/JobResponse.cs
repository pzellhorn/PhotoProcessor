namespace PhotoProcessor.DTO.ResponseDTOs.EntityResponses
{
    public class JobResponse
    {
        public JobResponse() { }

        public JobResponse(Guid? jobId, Guid mediaId, int jobType, int status)
        {
            JobId = jobId;
            MediaId = mediaId;
            JobType = jobType;
            Status = status;
        }

        public Guid? JobId { get; set; }

        public Guid MediaId { get; set; }

        public int JobType { get; set; }

        public int Status { get; set; }
    }
}
