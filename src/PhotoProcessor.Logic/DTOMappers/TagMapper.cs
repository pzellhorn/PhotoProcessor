using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.Logic.Base;

namespace PhotoProcessor.Logic.DTOMappers
{
    public class TagMapper
        : IDTOMapper<Tag, TagRequest, TagResponse>
    {
        public Guid? ExtractId(TagRequest request) => request.TagId;

        public void ApplyRequestToModel(TagRequest request, Tag model)
        {
            model.TagTypeId = request.TagTypeId;
            model.Label = request.Label;
        }

        public Tag CreateEntity(TagRequest request)
        => new()
        {
            TagTypeId = request.TagTypeId,
            Label = request.Label,
        };

        public TagResponse ToResponse(Tag model)
        => new(model.TagId, model.TagTypeId, model.Label);
    }
}
