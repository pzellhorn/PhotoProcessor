using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using PhotoProcessor.State.Data.Queries;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IFingerprintSearchLogic
    { 
        Task<List<FingerprintMatch>> SearchByFingerprint(Guid fingerprintId, int count, CancellationToken cancellationToken = default);
    }

    public class FingerprintSearchLogic(IFingerprintQueries fingerprintQueries, FingerprintLogic fingerprintLogic) : IFingerprintSearchLogic
    {
        public async Task<List<FingerprintMatch>> SearchByFingerprint(Guid fingerprintId, int count, CancellationToken cancellationToken = default)
        {
            Fingerprint source = await fingerprintLogic.Get(fingerprintId, cancellationToken) ?? throw new KeyNotFoundException($"Fingerprint {fingerprintId} not found.");

            List<FingerprintNeighbour> neighbours = await fingerprintQueries.NearestNeighbours(
                source.Embedding,
                count,
                excludeFingerprintId: fingerprintId,
                cancellationToken: cancellationToken);

            List<FingerprintMatch> matches = new();

            foreach (FingerprintNeighbour neighbour in neighbours)
            {
                FingerprintMatch match = new()
                {
                    FingerprintId = neighbour.Fingerprint.FingerprintId,
                    MediaId = neighbour.Fingerprint.MediaId,
                    TagId = neighbour.Fingerprint.TagId,
                    Distance = neighbour.Distance,
                };
                matches.Add(match);
            }

            return matches;
        }
    }
}
