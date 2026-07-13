using pzellhorn.Core.Logic.Base;
using pzellhorn.Core.State.Base.Interfaces;
using PhotoProcessor.State.Data.Entities;

namespace PhotoProcessor.Logic.EntityLogic
{
    public class VideoRenditionLogic(IBaseRepository<VideoRendition> videoRenditionRepository) : BaseLogic<VideoRendition>(videoRenditionRepository)
    {
    }
}
