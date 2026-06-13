using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.Logic.Base;

namespace PhotoProcessor.Logic.DTOMappers
{
    public class TagTypeMapper
        : IDTOMapper<TagType, TagTypeRequest, TagTypeResponse>
    {
        public Guid? ExtractId(TagTypeRequest request) => request.TagTypeId;

        public void ApplyRequestToModel(TagTypeRequest request, TagType model)
        {
            model.Name = request.Name;
        }

        public TagType CreateEntity(TagTypeRequest request)
        => new()
        {
            Name = request.Name,
        };

        public TagTypeResponse ToResponse(TagType model)
        => new(model.TagTypeId, model.Name);
    }
}
