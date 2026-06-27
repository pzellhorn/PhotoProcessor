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
        Task<Guid> EnqueueProcessing(Guid mediaId, JobTypes jobType, CancellationToken cancellationToken = default);
        Task<Uri> GetDownloadUrl(Guid mediaId, CancellationToken cancellationToken = default);
    }

    public class PhotoLogic(ISignedUrlProvider signedUrlProvider, IQueuePublisher queuePublisher, JobLogic jobLogic, MediaItemLogic mediaItemLogic) : IPhotoLogic
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

        /// <summary>
        /// Fires after a storage upload is completed via S3 bucket emitted event
        /// </summary>
        /// <param name="mediaId"></param>
        /// <param name="jobType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<Guid> EnqueueProcessing(Guid mediaId, JobTypes jobType, CancellationToken cancellationToken = default)
        {
            MediaItem media = await mediaItemLogic.Get(mediaId, cancellationToken) ?? throw new KeyNotFoundException($"Media {mediaId} not found.");

            Guid jobId = Guid.NewGuid();
            Job job = new()
            {
                JobId = jobId,
                MediaId = mediaId,
                JobType = (int)jobType,
                Status = (int)JobStatus.Queued,
            };
            await jobLogic.Upsert(job, cancellationToken);

            JobMessage message = new(
                JobId: jobId,
                MediaId: mediaId,
                JobType: jobType,
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
