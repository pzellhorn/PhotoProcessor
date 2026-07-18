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

    /// <summary> for jobs coming from video uploads, this is the video's media id that this image frame came from</summary>
    public Guid? ParentMediaId { get; set; } 
    /// <summary>Offset of this frame within its parent video.</summary>
    public double? TimestampMs { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }
     
    public static Expression<Func<MediaItem, Guid>> PrimaryKey => e => e.MediaItemId;

    public ICollection<Job> Jobs { get; set; } = [];
    public ICollection<TagItem> TagItems { get; set; } = [];
    public ICollection<Fingerprint> Fingerprints { get; set; } = [];

    public MediaItem? Parent { get; set; }
    public ICollection<MediaItem> Frames { get; set; } = [];
}

internal sealed class MediaItemConfig : BaseConfig<MediaItem>
{
    public override void Configure(EntityTypeBuilder<MediaItem> entity)
    {
        base.Configure(entity);

        entity.ToTable("media_items");
         
        entity.HasIndex(e => e.ContentHash)
            .IsUnique()
            .HasFilter("content_hash <> '' AND is_deleted = false");

        entity.HasOne(e => e.Parent)
            .WithMany(e => e.Frames)
            .HasForeignKey(e => e.ParentMediaId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(e => e.ParentMediaId);
    }
}
