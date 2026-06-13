using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.Interfaces;

namespace PhotoProcessor.State.Data.Entities;

public partial class TagItem : IIsDeleted, ICreatedAt, IModifiedAt, IPrimaryKeySelector<TagItem>
{
    public Guid TagItemId { get; set; }

    public Guid TagId { get; set; }

    public Guid MediaId { get; set; }

    public double? StartTimeMs { get; set; }

    public double? EndTimeMs { get; set; }

    public double Confidence { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }
     
    public static Expression<Func<TagItem, Guid>> PrimaryKey => e => e.TagItemId;
}

internal sealed class TagItemConfig : BaseConfig<TagItem>
{
    public override void Configure(EntityTypeBuilder<TagItem> entity)
    {
        base.Configure(entity);

        entity.ToTable("tag_items");
         
    }
}
