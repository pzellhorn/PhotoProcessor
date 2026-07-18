namespace PhotoProcessor.DTO.enums
{
    public static class JobMediaTypes
    {
        public static MediaItemType GetMediaTypeForJob(JobTypes jobType) => jobType switch
        {
            JobTypes.FaceRecognition => MediaItemType.Photo,
            JobTypes.ImageEmbedding => MediaItemType.Photo,
            JobTypes.VideoTranscode => MediaItemType.Video,
            _ => throw new ArgumentOutOfRangeException(nameof(jobType), jobType, "No media type mapping for job type."),
        };

        public static List<JobTypes> All() =>
        [
            JobTypes.FaceRecognition,
            JobTypes.VideoTranscode,
            JobTypes.ImageEmbedding,
        ];
    }
}
