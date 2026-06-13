using pzellhorn.Core.ClientAPI.Base;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;

namespace PhotoProcessor.ClientAPI
{
    public class TagTypeApi(ApiTransport api) : BaseClientApi<TagTypeRequest, TagTypeResponse>(api, "TagType")
    {
    }
}
