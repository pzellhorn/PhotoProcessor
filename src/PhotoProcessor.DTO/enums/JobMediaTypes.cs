namespace PhotoProcessor.DTO.enums
{
    public static class JobMediaTypes
    {
        public static List<MediaItemType> GetMediaTypesForJob(JobTypes jobType) => jobType switch
        {
            JobTypes.FaceRecognition => [MediaItemType.Photo, MediaItemType.Frame],
            JobTypes.ImageEmbedding => [MediaItemType.Photo, MediaItemType.Frame],
            JobTypes.VideoTranscode => [MediaItemType.Video],
            JobTypes.VideoKeyframes => [MediaItemType.Video],
            JobTypes.FlowerIdentification => [MediaItemType.Photo, MediaItemType.Frame],
            _ => throw new ArgumentOutOfRangeException(nameof(jobType), jobType, "No media type mapping for job type."),
        };

        public static List<JobTypes> ImageJobs() =>
        [
            JobTypes.FaceRecognition,
            JobTypes.ImageEmbedding,
            JobTypes.FlowerIdentification,
        ];

        public static List<JobTypes> All() =>
        [
            JobTypes.FaceRecognition,
            JobTypes.VideoTranscode,
            JobTypes.ImageEmbedding,
            JobTypes.VideoKeyframes,
            JobTypes.FlowerIdentification,
        ];
    }
}
