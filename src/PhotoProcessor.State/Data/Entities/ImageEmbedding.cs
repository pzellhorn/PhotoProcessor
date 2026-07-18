using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.Interfaces;

namespace PhotoProcessor.State.Data.Entities;

public partial class ImageEmbedding : IIsDeleted, ICreatedAt, IModifiedAt, IPrimaryKeySelector<ImageEmbedding>
{
    public const int EmbeddingDimensions = 512;

    public Guid ImageEmbeddingId { get; set; }
    public Guid MediaId { get; set; }
    public Guid? JobId { get; set; }

    public Vector Embedding { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    public static Expression<Func<ImageEmbedding, Guid>> PrimaryKey => e => e.ImageEmbeddingId;

    public MediaItem MediaItem { get; set; } = null!;
}

internal sealed class ImageEmbeddingConfig : BaseConfig<ImageEmbedding>
{
    public override void Configure(EntityTypeBuilder<ImageEmbedding> entity)
    {
        base.Configure(entity);

        entity.ToTable("image_embeddings");

        entity.Property(e => e.Embedding)
            .HasColumnType($"vector({ImageEmbedding.EmbeddingDimensions})");

        entity.HasOne(e => e.MediaItem)
            .WithMany()
            .HasForeignKey(e => e.MediaId);

        entity.HasIndex(e => e.MediaId);

        entity.HasIndex(e => e.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");
    }
}
