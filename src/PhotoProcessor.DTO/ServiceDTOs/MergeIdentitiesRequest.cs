namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class MergeIdentitiesRequest(Guid sourceTagId, Guid targetTagId)
    {
        public Guid SourceTagId { get; set; } = sourceTagId;
        public Guid TargetTagId { get; set; } = targetTagId;
    }
}
