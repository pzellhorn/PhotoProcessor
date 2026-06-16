using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.Messaging;
using pzellhorn.Core.State.Storage;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IPhotoLogic
    {
        Task IngestPhoto(string fileName, Stream stream, CancellationToken cancellationToken = default);
    }

    public class PhotoLogic(IStorageManager storageManager, IQueuePublisher queuePublisher, JobLogic jobLogic, MediaItemLogic mediaItemLogic) : IPhotoLogic
    {
        public async Task IngestPhoto(string fileName, Stream stream, CancellationToken cancellationToken = default)
        {
            Guid mediaId = Guid.NewGuid();
            string storageKey = $"photos/{mediaId}{Path.GetExtension(fileName)}";
             
            await storageManager.Upsert(storageKey, stream, cancellationToken);
             
            MediaItem media = new() { MediaItemId = mediaId, Uri = storageKey, MediaType = (int)MediaItemType.Photo };
            await mediaItemLogic.Upsert(media, cancellationToken);
              
            Guid jobId = Guid.NewGuid();
            Job job = new()
            {
                JobId = jobId,
                MediaId = mediaId,
                JobType = (int)JobTypes.FaceRecognition,
                Status = (int)JobStatus.Queued,
            };
            await jobLogic.Upsert(job, cancellationToken);

            JobMessage message = new(
                JobId: jobId,
                MediaId: mediaId,
                JobType: JobTypes.FaceRecognition,
                MediaUri: storageKey); 
            await queuePublisher.Publish("jobs", message, cancellationToken);
        }
    }
}
