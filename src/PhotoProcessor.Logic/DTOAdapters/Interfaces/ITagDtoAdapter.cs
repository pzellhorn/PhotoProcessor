using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using pzellhorn.Core.Logic.Base.DTOAdapter;

namespace PhotoProcessor.DTO.DTOAdapters.Interfaces
{
    public interface ITagDtoAdapter : IDtoLogicAdapter<TagRequest, TagResponse>
    {
    }
}
