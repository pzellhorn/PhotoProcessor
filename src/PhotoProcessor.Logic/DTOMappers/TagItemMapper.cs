using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.Logic.Base;

namespace PhotoProcessor.Logic.DTOMappers
{
    public class TagItemMapper
        : IDTOMapper<TagItem, TagItemRequest, TagItemResponse>
    {
        public Guid? ExtractId(TagItemRequest request) => request.TagItemId;

        public void ApplyRequestToModel(TagItemRequest request, TagItem model)
        {
            model.TagId = request.TagId;
            model.MediaId = request.MediaId;
            model.StartTimeMs = request.StartTimeMs;
            model.EndTimeMs = request.EndTimeMs;
            model.Confidence = request.Confidence;
        }

        public TagItem CreateEntity(TagItemRequest request)
        => new()
        {
            TagId = request.TagId,
            MediaId = request.MediaId,
            StartTimeMs = request.StartTimeMs,
            EndTimeMs = request.EndTimeMs,
            Confidence = request.Confidence,
        };

        public TagItemResponse ToResponse(TagItem model)
        => new(model.TagItemId, model.TagId, model.MediaId, model.StartTimeMs, model.EndTimeMs, model.Confidence);
    }
}
