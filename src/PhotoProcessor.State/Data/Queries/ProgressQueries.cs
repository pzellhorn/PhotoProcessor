using Microsoft.EntityFrameworkCore;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.State.Base.DBContext;

namespace PhotoProcessor.State.Data.Queries
{
    public record JobTypeCounts(int EligibleMedia, int Done, int Queued, int Running, int Failed, int NeverRun);

    public interface IProgressQueries
    {
        Task<JobTypeCounts> CountsFor(int jobType, List<int> mediaTypes, int doneStatus, int queuedStatus, int runningStatus, int failedStatus, CancellationToken cancellationToken = default);
        Task<List<Guid>> MediaNeverRun(int jobType, List<int> mediaTypes, CancellationToken cancellationToken = default);
    }

    public class ProgressQueries(BaseDbContext db) : IProgressQueries
    {
        public async Task<JobTypeCounts> CountsFor(int jobType, List<int> mediaTypes, int doneStatus, int queuedStatus, int runningStatus, int failedStatus, CancellationToken cancellationToken = default)
        {
            IQueryable<MediaItem> eligible = db.Set<MediaItem>().Where(m => mediaTypes.Contains(m.MediaType));
            IQueryable<Job> jobs = db.Set<Job>().Where(j => j.JobType == jobType);

            int eligibleMedia = await eligible.CountAsync(cancellationToken);
            int done = await eligible.CountAsync(m => jobs.Any(j => j.MediaId == m.MediaItemId && j.Status == doneStatus), cancellationToken);
            int queued = await eligible.CountAsync(m => jobs.Any(j => j.MediaId == m.MediaItemId && j.Status == queuedStatus), cancellationToken);
            int running = await eligible.CountAsync(m => jobs.Any(j => j.MediaId == m.MediaItemId && j.Status == runningStatus), cancellationToken);
            int failed = await eligible.CountAsync(m => jobs.Any(j => j.MediaId == m.MediaItemId && j.Status == failedStatus), cancellationToken);
            int neverRun = await eligible.CountAsync(m => !jobs.Any(j => j.MediaId == m.MediaItemId), cancellationToken);

            return new JobTypeCounts(eligibleMedia, done, queued, running, failed, neverRun);
        }

        public async Task<List<Guid>> MediaNeverRun(int jobType, List<int> mediaTypes, CancellationToken cancellationToken = default)
        {
            IQueryable<Job> jobs = db.Set<Job>().Where(j => j.JobType == jobType);

            return await db.Set<MediaItem>()
                .Where(m => mediaTypes.Contains(m.MediaType) && !jobs.Any(j => j.MediaId == m.MediaItemId))
                .Select(m => m.MediaItemId)
                .ToListAsync(cancellationToken);
        }
    }
}
