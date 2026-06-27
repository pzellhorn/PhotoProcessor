using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.Interfaces;

namespace PhotoProcessor.State.Data.Entities;

public partial class MediaItem : IIsDeleted, ICreatedAt, IModifiedAt, IPrimaryKeySelector<MediaItem>
{
    public Guid MediaItemId { get; set; }

    public int MediaType { get; set; }

    public string Uri { get; set; } = string.Empty;

    public string ThumbnailUri { get; set; } = string.Empty;

    public string ContentHash { get; set; } = string.Empty;

    public double? DurationMs { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }
     
    public static Expression<Func<MediaItem, Guid>> PrimaryKey => e => e.MediaItemId;

    public ICollection<Job> Jobs { get; set; } = [];
    public ICollection<TagItem> TagItems { get; set; } = [];
    public ICollection<Fingerprint> Fingerprints { get; set; } = [];
}

internal sealed class MediaItemConfig : BaseConfig<MediaItem>
{
    public override void Configure(EntityTypeBuilder<MediaItem> entity)
    {
        base.Configure(entity);

        entity.ToTable("media_items");
         
    }
}
