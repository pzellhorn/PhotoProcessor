using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using Pgvector;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IFingerprintSubmissionLogic
    {
        Task Submit(SubmitFingerprintsRequest request, CancellationToken cancellationToken = default);
    }

    public class FingerprintSubmissionLogic(JobLogic jobLogic, FingerprintLogic fingerprintLogic, IIdentityLogic identityLogic) : IFingerprintSubmissionLogic
    {
        public async Task Submit(SubmitFingerprintsRequest request, CancellationToken cancellationToken = default)
        {
            Job job = await jobLogic.Get(request.JobId, cancellationToken) ?? throw new KeyNotFoundException($"Job {request.JobId} not found.");
            Guid mediaId = job.MediaId;

            foreach (DetectedFace face in request.Faces)
            {
                if (face.Embedding.Length != Fingerprint.EmbeddingDimensions)
                    throw new ArgumentException($"Embedding must have {Fingerprint.EmbeddingDimensions} dimensions but had {face.Embedding.Length}.", nameof(request));
            }
             
            List<Fingerprint> existing = await fingerprintLogic.GetFor(mediaId, f => f.MediaId, cancellationToken);
            foreach (Fingerprint fingerprint in existing)
                await fingerprintLogic.Delete(fingerprint.FingerprintId, cancellationToken);

            foreach (DetectedFace face in request.Faces)
            {
                Fingerprint fingerprint = new()
                {
                    FingerprintId = Guid.NewGuid(),
                    MediaId = mediaId,
                    JobId = job.JobId,
                    Embedding = new Vector(face.Embedding),
                    DetectionScore = face.DetectionScore,
                    BoundingX = face.BoundingX,
                    BoundingY = face.BoundingY,
                    BoundingWidth = face.BoundingWidth,
                    BoundingHeight = face.BoundingHeight,
                };
                await fingerprintLogic.Upsert(fingerprint, cancellationToken);
            }
             
            await identityLogic.AssignForMedia(mediaId, cancellationToken);

            await jobLogic.MarkDone(job, cancellationToken);
        }
    }
}
