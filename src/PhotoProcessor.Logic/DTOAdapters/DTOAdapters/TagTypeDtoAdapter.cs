using PhotoProcessor.DTO.DTOAdapters.Interfaces;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.Logic.Base;
using pzellhorn.Core.Logic.Base.DTOAdapter;

namespace PhotoProcessor.Logic.DTOAdapters.DTOAdapters
{
    public class TagTypeDtoAdapter(
        TagTypeLogic tagTypeLogic,
        IDTOMapper<TagType, TagTypeRequest, TagTypeResponse> mapper)
        : DtoLogicAdapter<TagType, TagTypeRequest, TagTypeResponse>(tagTypeLogic, mapper),
          ITagTypeDtoAdapter
    {
    }
}
