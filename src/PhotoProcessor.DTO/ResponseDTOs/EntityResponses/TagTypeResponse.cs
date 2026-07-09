namespace PhotoProcessor.DTO.ResponseDTOs.EntityResponses
{
    public class TagTypeResponse
    {
        public TagTypeResponse() { }

        public TagTypeResponse(Guid? tagTypeId)
        {
            TagTypeId = tagTypeId;
        }

        public Guid? TagTypeId { get; set; }
    }
}
