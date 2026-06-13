using pzellhorn.Core.ClientAPI.Base;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;

namespace PhotoProcessor.ClientAPI
{
    public class MediaItemApi(ApiTransport api) : BaseClientApi<MediaItemRequest, MediaItemResponse>(api, "MediaItem")
    {
    }
}
