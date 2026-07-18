namespace PhotoProcessor.DTO.enums
{
    public static class JobQueues
    {
        public static string GetQueueForJob(JobTypes jobType) => jobType switch
        {
            JobTypes.FaceRecognition => "jobs.face-recognition",
            JobTypes.VideoTranscode => "jobs.video-transcode",
            JobTypes.ImageEmbedding => "jobs.image-embedding",
            JobTypes.VideoKeyframes => "jobs.video-keyframes",
            _ => throw new ArgumentOutOfRangeException(nameof(jobType), jobType, "No queue mapping for job type."),
        };
    }
}
