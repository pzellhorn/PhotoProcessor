using pzellhorn.Core.Logic.Base;
using pzellhorn.Core.State.Base.Interfaces;
using PhotoProcessor.State.Data.Entities;

namespace PhotoProcessor.Logic.EntityLogic
{
    public class FlowerIdentificationLogic(IBaseRepository<FlowerIdentification> flowerIdentificationRepository, FlowerPredictionLogic flowerPredictionLogic) : BaseLogic<FlowerIdentification>(flowerIdentificationRepository)
    {
        public async Task DeleteForMedia(Guid mediaId, CancellationToken cancellationToken = default)
        {
            List<FlowerIdentification> identifications = await GetFor(mediaId, f => f.MediaId, cancellationToken);
            foreach (FlowerIdentification identification in identifications)
            {
                List<FlowerPrediction> predictions = await flowerPredictionLogic.GetFor(identification.FlowerIdentificationId, p => p.FlowerIdentificationId, cancellationToken);
                foreach (FlowerPrediction prediction in predictions)
                    await flowerPredictionLogic.Delete(prediction.FlowerPredictionId, cancellationToken);

                await Delete(identification.FlowerIdentificationId, cancellationToken);
            }
        }
    }
}
