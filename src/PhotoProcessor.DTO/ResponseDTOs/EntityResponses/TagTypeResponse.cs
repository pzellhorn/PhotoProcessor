namespace PhotoProcessor.DTO.ResponseDTOs.EntityResponses
{
    public class TagTypeResponse
    {
        public TagTypeResponse() { }

        public TagTypeResponse(Guid? tagTypeId, string name)
        {
            TagTypeId = tagTypeId;
            Name = name;
        }

        public Guid? TagTypeId { get; set; }

        public string Name { get; set; }
    }
}
