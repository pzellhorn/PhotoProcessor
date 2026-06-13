namespace PhotoProcessor.DTO.RequestDTOs.EntityRequests
{
    public class TagTypeRequest
    {
        public TagTypeRequest() { }

        public TagTypeRequest(Guid? tagTypeId, string name)
        {
            TagTypeId = tagTypeId;
            Name = name;
        }

        public Guid? TagTypeId { get; set; }

        public string Name { get; set; }
    }
}
