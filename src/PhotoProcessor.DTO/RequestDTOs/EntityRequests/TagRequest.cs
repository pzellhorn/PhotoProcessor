namespace PhotoProcessor.DTO.RequestDTOs.EntityRequests
{
    public class TagRequest
    {
        public TagRequest() { }

        public TagRequest(Guid? tagId, Guid tagTypeId, string label)
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
