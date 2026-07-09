using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.Interfaces;

namespace PhotoProcessor.State.Data.Entities;

public partial class Fingerprint : IIsDeleted, ICreatedAt, IModifiedAt, IPrimaryKeySelector<Fingerprint>
{
    public const int EmbeddingDimensions = 512;

    public Guid FingerprintId { get; set; } 
    public Guid MediaId { get; set; } 
    public Guid? JobId { get; set; } 
    public Guid? TagId { get; set; }

    public Vector Embedding { get; set; } = null!;
     
    public double? DetectionScore { get; set; }
     
    public double? BoundingX { get; set; }
    public double? BoundingY { get; set; }
    public double? BoundingWidth { get; set; }
    public double? BoundingHeight { get; set; }

    public bool IsDeleted { get; set; } 
    public DateTime CreatedAt { get; set; } 
    public DateTime ModifiedAt { get; set; }

    public static Expression<Func<Fingerprint, Guid>> PrimaryKey => e => e.FingerprintId;

    public MediaItem MediaItem { get; set; } = null!; 
    public Tag? Tag { get; set; }
}

internal sealed class FingerprintConfig : BaseConfig<Fingerprint>
{
    public override void Configure(EntityTypeBuilder<Fingerprint> entity)
    {
        base.Configure(entity);

        entity.ToTable("fingerprints");

        entity.Property(e => e.Embedding)
            .HasColumnType($"vector({Fingerprint.EmbeddingDimensions})");

        entity.HasOne(e => e.MediaItem)
            .WithMany(e => e.Fingerprints)
            .HasForeignKey(e => e.MediaId);

        entity.HasOne(e => e.Tag)
            .WithMany()
            .HasForeignKey(e => e.TagId);

        // Approximate-nearest-neighbour index for cosine-similarity search over embeddings.
        entity.HasIndex(e => e.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");
    }
}
