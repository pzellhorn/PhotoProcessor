using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.Interfaces;

namespace PhotoProcessor.State.Data.Entities;

public partial class FlowerIdentification : IIsDeleted, ICreatedAt, IModifiedAt, IPrimaryKeySelector<FlowerIdentification>
{
    public const int EmbeddingDimensions = 768;

    public Guid FlowerIdentificationId { get; set; }
    public Guid MediaId { get; set; }
    public Guid? JobId { get; set; }

    public Vector Embedding { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    public static Expression<Func<FlowerIdentification, Guid>> PrimaryKey => e => e.FlowerIdentificationId;

    public MediaItem MediaItem { get; set; } = null!;
    public List<FlowerPrediction> Predictions { get; set; } = new();
}

internal sealed class FlowerIdentificationConfig : BaseConfig<FlowerIdentification>
{
    public override void Configure(EntityTypeBuilder<FlowerIdentification> entity)
    {
        base.Configure(entity);

        entity.ToTable("flower_identifications");

        entity.Property(e => e.Embedding)
            .HasColumnType($"vector({FlowerIdentification.EmbeddingDimensions})");

        entity.HasOne(e => e.MediaItem)
            .WithMany()
            .HasForeignKey(e => e.MediaId);

        // One live result per media item: a racing duplicate submit fails and its job retries.
        entity.HasIndex(e => e.MediaId)
            .IsUnique()
            .HasFilter("is_deleted = false");

        entity.HasIndex(e => e.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");
    }
}
