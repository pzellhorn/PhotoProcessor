using pzellhorn.Core.ClientAPI.Base;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;

namespace PhotoProcessor.ClientAPI
{
    public class TagApi(ApiTransport api) : BaseClientApi<TagRequest, TagResponse>(api, "Tag")
    {
    }
}
