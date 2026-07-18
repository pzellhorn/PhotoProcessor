using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.State.Base.DBContext;

namespace PhotoProcessor.State.Data.Queries
{
    public record ImageEmbeddingNeighbour(ImageEmbedding ImageEmbedding, double Distance);

    public interface IImageEmbeddingQueries
    {
        Task<List<ImageEmbeddingNeighbour>> NearestNeighbours(
            Vector vectorQuery,
            int count,
            CancellationToken cancellationToken = default);
    }

    public class ImageEmbeddingQueries(BaseDbContext db) : IImageEmbeddingQueries
    {
        public async Task<List<ImageEmbeddingNeighbour>> NearestNeighbours(
            Vector vectorQuery,
            int count,
            CancellationToken cancellationToken = default)
        {
            var ranked = await db.Set<ImageEmbedding>()
                .Select(e => new { ImageEmbedding = e, Distance = e.Embedding.CosineDistance(vectorQuery) })
                .OrderBy(x => x.Distance)
                .Take(count)
                .ToListAsync(cancellationToken);

            List<ImageEmbeddingNeighbour> neighbours = new();

            foreach (var row in ranked)
            {
                ImageEmbeddingNeighbour neighbour = new(row.ImageEmbedding, row.Distance);
                neighbours.Add(neighbour);
            }

            return neighbours;
        }
    }
}
