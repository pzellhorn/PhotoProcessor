using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.Interfaces;

namespace PhotoProcessor.State.Data.Entities;

public partial class Job : IIsDeleted, ICreatedAt, IModifiedAt, IPrimaryKeySelector<Job>
{
    public Guid JobId { get; set; }

    public Guid MediaId { get; set; }

    public int JobType { get; set; }

    public int Status { get; set; }

    public string? Error { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }
     
    public static Expression<Func<Job, Guid>> PrimaryKey => e => e.JobId;

    public MediaItem MediaItem { get; set; } = null!;
}

internal sealed class JobConfig : BaseConfig<Job>
{
    public override void Configure(EntityTypeBuilder<Job> entity)
    {
        base.Configure(entity);

        entity.ToTable("jobs");

        entity.HasOne(e => e.MediaItem)
            .WithMany(e => e.Jobs)
            .HasForeignKey(e => e.MediaId);
    }
}
