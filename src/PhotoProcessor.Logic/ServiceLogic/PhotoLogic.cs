using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.Messaging;
using pzellhorn.Core.State.Storage;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IPhotoLogic
    {
        Task<(Guid MediaId, Uri UploadUrl)> CreateUpload(string fileName, CancellationToken cancellationToken = default);
        Task<Guid> CompleteUpload(Guid mediaId, CancellationToken cancellationToken = default);
        Task<Uri> GetDownloadUrl(Guid mediaId, CancellationToken cancellationToken = default);
    }

    public class PhotoLogic(IStorageManager storageManager, ISignedUrlProvider signedUrlProvider, IQueuePublisher queuePublisher, JobLogic jobLogic, MediaItemLogic mediaItemLogic) : IPhotoLogic
    {
        private static readonly TimeSpan UrlLifetime = TimeSpan.FromMinutes(30);
         
        public async Task<(Guid MediaId, Uri UploadUrl)> CreateUpload(string fileName, CancellationToken cancellationToken = default)
        {
            Guid mediaId = Guid.NewGuid();
            string storageKey = $"photos/{mediaId}{Path.GetExtension(fileName)}";

            MediaItem media = new() { MediaItemId = mediaId, Uri = storageKey, MediaType = (int)MediaItemType.Photo };
            await mediaItemLogic.Upsert(media, cancellationToken);

            Uri uploadUrl = await signedUrlProvider.GetUploadUrl(storageKey, UrlLifetime, cancellationToken);
            return (mediaId, uploadUrl);
        }
         
        public async Task<Guid> CompleteUpload(Guid mediaId, CancellationToken cancellationToken = default)
        {
            MediaItem media = await mediaItemLogic.Get(mediaId, cancellationToken) ?? throw new KeyNotFoundException($"Media {mediaId} not found.");

            if (!await storageManager.Exists(media.Uri, cancellationToken))
                throw new InvalidOperationException($"No uploaded object found for media {mediaId}.");

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
                MediaUri: media.Uri);
            await queuePublisher.Publish("jobs", message, cancellationToken);

            return jobId;
        }

        public async Task<Uri> GetDownloadUrl(Guid mediaId, CancellationToken cancellationToken = default)
        {
            MediaItem media = await mediaItemLogic.Get(mediaId, cancellationToken) ?? throw new KeyNotFoundException($"Media {mediaId} not found.");

            return await signedUrlProvider.GetDownloadUrl(media.Uri, UrlLifetime, cancellationToken);
        }
    }
}
