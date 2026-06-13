using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.Interfaces;

namespace PhotoProcessor.State.Data.Entities;

public partial class TagType : IIsDeleted, ICreatedAt, IModifiedAt, IPrimaryKeySelector<TagType>
{
    public Guid TagTypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }
     
    public static Expression<Func<TagType, Guid>> PrimaryKey => e => e.TagTypeId;

    public ICollection<Tag> Tags { get; set; } = [];
}

internal sealed class TagTypeConfig : BaseConfig<TagType>
{
    public override void Configure(EntityTypeBuilder<TagType> entity)
    {
        base.Configure(entity);

        entity.ToTable("tag_types");
         
    }
}
