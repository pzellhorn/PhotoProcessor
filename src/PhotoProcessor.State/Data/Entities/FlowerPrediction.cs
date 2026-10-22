using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.Interfaces;

namespace PhotoProcessor.State.Data.Entities;

public partial class FlowerPrediction : IIsDeleted, ICreatedAt, IModifiedAt, IPrimaryKeySelector<FlowerPrediction>
{
    public Guid FlowerPredictionId { get; set; }
    public Guid FlowerIdentificationId { get; set; }

    public int Rank { get; set; }
    public string ScientificName { get; set; } = string.Empty;
    public string? CommonName { get; set; }
    public string Family { get; set; } = string.Empty;
    public double Score { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    public static Expression<Func<FlowerPrediction, Guid>> PrimaryKey => e => e.FlowerPredictionId;

    public FlowerIdentification FlowerIdentification { get; set; } = null!;
}

internal sealed class FlowerPredictionConfig : BaseConfig<FlowerPrediction>
{
    public override void Configure(EntityTypeBuilder<FlowerPrediction> entity)
    {
        base.Configure(entity);

        entity.ToTable("flower_predictions");

        entity.HasOne(e => e.FlowerIdentification)
            .WithMany(e => e.Predictions)
            .HasForeignKey(e => e.FlowerIdentificationId);

        entity.HasIndex(e => e.FlowerIdentificationId);
    }
}
