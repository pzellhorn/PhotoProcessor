namespace PhotoProcessor.DTO.ResponseDTOs.EntityResponses
{
    public class TagResponse
    {
        public TagResponse() { }

        public TagResponse(Guid? tagId, Guid tagTypeId, string label)
        {
            TagId = tagId;
            TagTypeId = tagTypeId;
            Label = label;
        }

        public Guid? TagId { get; set; }

        public Guid TagTypeId { get; set; }

        public string Label { get; set; }
    }
}
