using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.Interfaces;

namespace PhotoProcessor.State.Data.Entities;

public partial class UploadSession : IIsDeleted, ICreatedAt, IModifiedAt, IPrimaryKeySelector<UploadSession>
{
    public Guid UploadSessionId { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string StagingKey { get; set; } = string.Empty;
    public string MultipartUploadId { get; set; } = string.Empty;

    public int MediaType { get; set; }

    public long TotalBytes { get; set; }
    public long ReceivedBytes { get; set; }
    public int NextPartNumber { get; set; }

    public bool Completed { get; set; }
    public bool Duplicate { get; set; }
    public Guid? MediaId { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    public static Expression<Func<UploadSession, Guid>> PrimaryKey => e => e.UploadSessionId;
}

internal sealed class UploadSessionConfig : BaseConfig<UploadSession>
{
    public override void Configure(EntityTypeBuilder<UploadSession> entity)
    {
        base.Configure(entity);

        entity.ToTable("upload_sessions");
    }
}
