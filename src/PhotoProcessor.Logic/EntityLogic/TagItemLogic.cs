using pzellhorn.Core.Logic.Base;
using pzellhorn.Core.State.Base.Interfaces;
using PhotoProcessor.State.Data.Entities;

namespace PhotoProcessor.Logic.EntityLogic
{
    public class TagItemLogic(IBaseRepository<TagItem> tagItemRepository) : BaseLogic<TagItem>(tagItemRepository)
    {
    }
}
