using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IFlowerIdentificationQueryLogic
    {
        Task<FlowerIdentificationSummary?> GetForMedia(Guid mediaId, CancellationToken cancellationToken = default);
    }

    public class FlowerIdentificationQueryLogic(FlowerIdentificationLogic flowerIdentificationLogic, FlowerPredictionLogic flowerPredictionLogic, ImageEmbeddingLogic imageEmbeddingLogic, IFlowerGate flowerGate) : IFlowerIdentificationQueryLogic
    {
        public const double FlowerScoreThreshold = 0.5;

        public async Task<FlowerIdentificationSummary?> GetForMedia(Guid mediaId, CancellationToken cancellationToken = default)
        {
            List<FlowerIdentification> identifications = await flowerIdentificationLogic.GetFor(mediaId, f => f.MediaId, cancellationToken);
            if (identifications.Count == 0)
                return null;
             
            List<ImageEmbedding> imageEmbeddings = await imageEmbeddingLogic.GetFor(mediaId, e => e.MediaId, cancellationToken);
            double? flowerScore = imageEmbeddings.Count == 0 ? null : await flowerGate.Score(imageEmbeddings[0].Embedding.ToArray(), cancellationToken);

            FlowerIdentification identification = identifications[0];
            FlowerIdentificationSummary summary = new()
            {
                MediaId = mediaId,
                IsFlower = flowerScore >= FlowerScoreThreshold,
                FlowerScore = flowerScore,
            };
             
            if (!summary.IsFlower)
                return summary;

            List<FlowerPrediction> predictions = await flowerPredictionLogic.GetFor(identification.FlowerIdentificationId, p => p.FlowerIdentificationId, cancellationToken);
            predictions.Sort((a, b) => a.Rank.CompareTo(b.Rank));

            foreach (FlowerPrediction prediction in predictions)
            {
                summary.Predictions.Add(new FlowerPredictionDto
                {
                    ScientificName = prediction.ScientificName,
                    CommonName = prediction.CommonName,
                    Family = prediction.Family,
                    Score = prediction.Score,
                });
            }
            return summary;
        }
    }
}
