using Microsoft.EntityFrameworkCore;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.State.Base.DBContext;

namespace PhotoProcessor.State.Data.Queries
{
    public interface IMediaQueries
    { 
        Task<(List<MediaItem> Items, int TotalCount)> ListLibrary(int page, int pageSize, CancellationToken cancellationToken = default);
    }

    public class MediaQueries(BaseDbContext db) : IMediaQueries
    {
        public async Task<(List<MediaItem> Items, int TotalCount)> ListLibrary(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            IQueryable<MediaItem> query = db.Set<MediaItem>().Where(m => m.ParentMediaId == null);

            int totalCount = await query.CountAsync(cancellationToken);

            List<MediaItem> items = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
