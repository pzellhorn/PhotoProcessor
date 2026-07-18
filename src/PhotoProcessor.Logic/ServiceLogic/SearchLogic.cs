using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.Encoding;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using PhotoProcessor.State.Data.Queries;
using Pgvector;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface ISearchLogic
    {
        Task<List<MediaSearchResult>> SearchByText(string text, int count, CancellationToken cancellationToken = default);
    }

    public class SearchLogic(ITextEncoder textEncoder, IImageEmbeddingQueries imageEmbeddingQueries, MediaItemLogic mediaItemLogic) : ISearchLogic
    {
        public async Task<List<MediaSearchResult>> SearchByText(string text, int count, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Search text must not be empty.", nameof(text));

            float[] embedding = await textEncoder.EncodeText(text, cancellationToken);

            if (embedding.Length != ImageEmbedding.EmbeddingDimensions)
                throw new InvalidOperationException($"Text encoder returned {embedding.Length} dimensions but {ImageEmbedding.EmbeddingDimensions} were expected.");

            List<ImageEmbeddingNeighbour> neighbours = await imageEmbeddingQueries.NearestNeighbours(new Vector(embedding), count, cancellationToken);

            List<MediaSearchResult> results = new();
            foreach (ImageEmbeddingNeighbour neighbour in neighbours)
            {
                MediaItem? media = await mediaItemLogic.Get(neighbour.ImageEmbedding.MediaId, cancellationToken);
                if (media is null)
                    continue;

                results.Add(new MediaSearchResult
                {
                    MediaId = media.MediaItemId,
                    MediaType = (MediaItemType)media.MediaType,
                    DurationMs = media.DurationMs,
                    Distance = neighbour.Distance,
                });
            }

            return results;
        }
    }
}
