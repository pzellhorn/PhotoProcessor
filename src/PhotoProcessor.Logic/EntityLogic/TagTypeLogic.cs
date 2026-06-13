using pzellhorn.Core.Logic.Base;
using pzellhorn.Core.State.Base.Interfaces;
using PhotoProcessor.State.Data.Entities;

namespace PhotoProcessor.Logic.EntityLogic
{
    public class TagTypeLogic(IBaseRepository<TagType> tagTypeRepository) : BaseLogic<TagType>(tagTypeRepository)
    {
    }
}
