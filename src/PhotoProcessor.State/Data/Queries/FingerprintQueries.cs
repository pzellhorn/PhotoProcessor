using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.State.Base.DBContext;

namespace PhotoProcessor.State.Data.Queries
{ 
    public record FingerprintNeighbour(Fingerprint Fingerprint, double Distance);

    public interface IFingerprintQueries
    { 
        Task<List<FingerprintNeighbour>> NearestNeighbours(
            Vector vectorQuery,
            int count,
            Guid? excludeFingerprintId = null,
            Guid? excludeMediaId = null,
            bool assignedOnly = false,
            CancellationToken cancellationToken = default);
         
        Task<Dictionary<Guid, int>> FaceCountsByTag(CancellationToken cancellationToken = default);
    }

    public class FingerprintQueries(BaseDbContext db) : IFingerprintQueries
    {
        public async Task<List<FingerprintNeighbour>> NearestNeighbours(
            Vector vectorQuery,
            int count,
            Guid? excludeFingerprintId = null,
            Guid? excludeMediaId = null,
            bool assignedTagsOnly = false,
            CancellationToken cancellationToken = default)
        { 
            IQueryable<Fingerprint> query = db.Set<Fingerprint>();

            if (excludeFingerprintId is Guid fingerprintId)
                query = query.Where(f => f.FingerprintId != fingerprintId);
            if (excludeMediaId is Guid mediaId)
                query = query.Where(f => f.MediaId != mediaId);
            if (assignedTagsOnly)
                query = query.Where(f => f.TagId != null);
             
            var ranked = await query
                .Select(f => new { Fingerprint = f, Distance = f.Embedding.CosineDistance(vectorQuery) })
                .OrderBy(x => x.Distance)
                .Take(count)
                .ToListAsync(cancellationToken);

            List<FingerprintNeighbour> neighbours = new();

            foreach (var row in ranked)
            {
                FingerprintNeighbour neighbour = new(row.Fingerprint, row.Distance);
                neighbours.Add(neighbour);
            }

            return neighbours;
        }

        public async Task<Dictionary<Guid, int>> FaceCountsByTag(CancellationToken cancellationToken = default)
        {
            var counts = await db.Set<Fingerprint>()
                .Where(f => f.TagId != null)
                .GroupBy(f => f.TagId!.Value)
                .Select(g => new { TagId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            return counts.ToDictionary(x => x.TagId, x => x.Count);
        }
    }
}
