namespace PhotoProcessor.DTO.RequestDTOs.EntityRequests
{
    public class TagTypeRequest
    {
        public TagTypeRequest() { }

        public TagTypeRequest(Guid? tagTypeId)
        {
            TagTypeId = tagTypeId;
        }

        public Guid? TagTypeId { get; set; }
    }
}
