using Microsoft.EntityFrameworkCore;
using pzellhorn.Core.State.Base.DBContext;

namespace PhotoProcessor.State;

public class PhotoProcessorDbContext : BaseDbContext
{
    public PhotoProcessorDbContext(DbContextOptions<PhotoProcessorDbContext> options) : base(options) { }
}
