using pzellhorn.Core.ClientAPI.Base;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;

namespace PhotoProcessor.ClientAPI
{
    public class TagItemApi(ApiTransport api) : BaseClientApi<TagItemRequest, TagItemResponse>(api, "TagItem")
    {
    }
}
