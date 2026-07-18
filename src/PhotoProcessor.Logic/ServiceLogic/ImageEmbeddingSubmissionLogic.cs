using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using Pgvector;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IImageEmbeddingSubmissionLogic
    {
        Task Submit(SubmitImageEmbeddingRequest request, CancellationToken cancellationToken = default);
    }

    public class ImageEmbeddingSubmissionLogic(JobLogic jobLogic, ImageEmbeddingLogic imageEmbeddingLogic) : IImageEmbeddingSubmissionLogic
    {
        public async Task Submit(SubmitImageEmbeddingRequest request, CancellationToken cancellationToken = default)
        {
            Job job = await jobLogic.Get(request.JobId, cancellationToken) ?? throw new KeyNotFoundException($"Job {request.JobId} not found.");
            Guid mediaId = job.MediaId;

            if (request.Embedding.Length != ImageEmbedding.EmbeddingDimensions)
                throw new ArgumentException($"Embedding must have {ImageEmbedding.EmbeddingDimensions} dimensions but had {request.Embedding.Length}.", nameof(request));

            List<ImageEmbedding> existing = await imageEmbeddingLogic.GetFor(mediaId, e => e.MediaId, cancellationToken);
            foreach (ImageEmbedding embedding in existing)
                await imageEmbeddingLogic.Delete(embedding.ImageEmbeddingId, cancellationToken);

            ImageEmbedding created = new()
            {
                ImageEmbeddingId = Guid.NewGuid(),
                MediaId = mediaId,
                JobId = job.JobId,
                Embedding = new Vector(request.Embedding),
            };
            await imageEmbeddingLogic.Upsert(created, cancellationToken);

            job.Status = (int)JobStatus.Done;
            await jobLogic.Upsert(job, cancellationToken);
        }
    }
}
