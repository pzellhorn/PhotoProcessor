namespace PhotoProcessor.DTO.ResponseDTOs.EntityResponses
{
    public class TagItemResponse
    {
        public TagItemResponse() { }

        public TagItemResponse(Guid? tagItemId, Guid tagId, Guid mediaId, double? startTimeMs, double? endTimeMs, double confidence)
        {
            TagItemId = tagItemId;
            TagId = tagId;
            MediaId = mediaId;
            StartTimeMs = startTimeMs;
            EndTimeMs = endTimeMs;
            Confidence = confidence;
        }

        public Guid? TagItemId { get; set; }

        public Guid TagId { get; set; }

        public Guid MediaId { get; set; }

        public double? StartTimeMs { get; set; }

        public double? EndTimeMs { get; set; }

        public double Confidence { get; set; }
    }
}
