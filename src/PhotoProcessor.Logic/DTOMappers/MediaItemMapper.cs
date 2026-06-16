using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.Logic.Base;

namespace PhotoProcessor.Logic.DTOMappers
{
    public class MediaItemMapper
        : IDTOMapper<MediaItem, MediaItemRequest, MediaItemResponse>
    {
        public Guid? ExtractId(MediaItemRequest request) => request.MediaItemId;

        public void ApplyRequestToModel(MediaItemRequest request, MediaItem model)
        {
            model.MediaType = (int)request.MediaType;
            model.Uri = request.Uri;
            model.ThumbnailUri = request.ThumbnailUri;
            model.ContentHash = request.ContentHash;
            model.DurationMs = request.DurationMs;
        }

        public MediaItem CreateEntity(MediaItemRequest request)
        => new()
        {
            MediaType = (int)request.MediaType,
            Uri = request.Uri,
            ThumbnailUri = request.ThumbnailUri,
            ContentHash = request.ContentHash,
            DurationMs = request.DurationMs,
        };

        public MediaItemResponse ToResponse(MediaItem model)
        => new(model.MediaItemId, (MediaItemType)model.MediaType, model.Uri, model.ThumbnailUri, model.ContentHash, model.DurationMs);
    }
}
