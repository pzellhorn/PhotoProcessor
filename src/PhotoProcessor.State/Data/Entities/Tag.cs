using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.Interfaces;

namespace PhotoProcessor.State.Data.Entities;

public partial class Tag : IIsDeleted, ICreatedAt, IModifiedAt, IPrimaryKeySelector<Tag>
{
    public Guid TagId { get; set; }

    public Guid TagTypeId { get; set; }

    public string Label { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }
     
    public static Expression<Func<Tag, Guid>> PrimaryKey => e => e.TagId;

    public TagType TagType { get; set; } = null!;
    public ICollection<TagItem> TagItems { get; set; } = [];
}

internal sealed class TagConfig : BaseConfig<Tag>
{
    public override void Configure(EntityTypeBuilder<Tag> entity)
    {
        base.Configure(entity);

        entity.ToTable("tags");

        entity.HasOne(e => e.TagType)
            .WithMany(e => e.Tags)
            .HasForeignKey(e => e.TagTypeId);
    }
}
