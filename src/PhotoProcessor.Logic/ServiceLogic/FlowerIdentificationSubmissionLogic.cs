using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using Pgvector;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IFlowerIdentificationSubmissionLogic
    {
        Task Submit(SubmitFlowerIdentificationRequest request, CancellationToken cancellationToken = default);
    }

    public class FlowerIdentificationSubmissionLogic(JobLogic jobLogic, FlowerIdentificationLogic flowerIdentificationLogic) : IFlowerIdentificationSubmissionLogic
    {
        public async Task Submit(SubmitFlowerIdentificationRequest request, CancellationToken cancellationToken = default)
        {
            Job job = await jobLogic.Get(request.JobId, cancellationToken) ?? throw new KeyNotFoundException($"Job {request.JobId} not found.");
            Guid mediaId = job.MediaId;

            if (request.Embedding.Length != FlowerIdentification.EmbeddingDimensions)
                throw new ArgumentException($"Embedding must have {FlowerIdentification.EmbeddingDimensions} dimensions but had {request.Embedding.Length}.", nameof(request));

            await flowerIdentificationLogic.DeleteForMedia(mediaId, cancellationToken);

            FlowerIdentification identification = new()
            {
                FlowerIdentificationId = Guid.NewGuid(),
                MediaId = mediaId,
                JobId = job.JobId,
                Embedding = new Vector(request.Embedding),
            };

            int rank = 1;
            foreach (FlowerPredictionDto prediction in request.Predictions)
            {
                identification.Predictions.Add(new FlowerPrediction
                {
                    FlowerPredictionId = Guid.NewGuid(),
                    FlowerIdentificationId = identification.FlowerIdentificationId,
                    Rank = rank++,
                    ScientificName = prediction.ScientificName,
                    CommonName = prediction.CommonName,
                    Family = prediction.Family,
                    Score = prediction.Score,
                });
            }
             
            await flowerIdentificationLogic.Upsert(identification, cancellationToken); 
            await jobLogic.MarkDone(job, cancellationToken);
        }
    }
}
