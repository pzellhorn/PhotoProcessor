using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.Interfaces;

namespace PhotoProcessor.State.Data.Entities;

public partial class VideoRendition : IIsDeleted, ICreatedAt, IModifiedAt, IPrimaryKeySelector<VideoRendition>
{
    public Guid RenditionId { get; set; }
    public Guid MediaId { get; set; }

    public string Format { get; set; } = string.Empty;
    public string EntryPath { get; set; } = string.Empty;

    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Bitrate { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    public static Expression<Func<VideoRendition, Guid>> PrimaryKey => e => e.RenditionId;

    public MediaItem MediaItem { get; set; } = null!;
}

internal sealed class VideoRenditionConfig : BaseConfig<VideoRendition>
{
    public override void Configure(EntityTypeBuilder<VideoRendition> entity)
    {
        base.Configure(entity);

        entity.ToTable("video_renditions");

        entity.HasKey(e => e.RenditionId);

        entity.HasOne(e => e.MediaItem)
            .WithMany()
            .HasForeignKey(e => e.MediaId);

        entity.HasIndex(e => e.MediaId);
    }
}
