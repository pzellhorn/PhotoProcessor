using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.Messaging;
using pzellhorn.Core.State.Storage;
using SkiaSharp;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IPhotoLogic
    {
        Task<Guid> EnqueueProcessing(Guid mediaId, JobTypes jobType, CancellationToken cancellationToken = default);
        Task<Uri> GetDownloadUrl(Guid mediaId, CancellationToken cancellationToken = default);

        Task<Guid> Upload(string fileName, Stream content, CancellationToken cancellationToken = default);
        Task<(Stream Stream, string ContentType)> GetImage(Guid mediaId, CancellationToken cancellationToken = default);
        Task<Stream> GetThumbnail(Guid mediaId, int width, CancellationToken cancellationToken = default);
    }

    public class PhotoLogic(ISignedUrlProvider signedUrlProvider, IStorageManager storageManager, IQueuePublisher queuePublisher, JobLogic jobLogic, MediaItemLogic mediaItemLogic) : IPhotoLogic
    {
        private static readonly TimeSpan UrlLifetime = TimeSpan.FromMinutes(30);

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

        public async Task<Guid> Upload(string fileName, Stream content, CancellationToken cancellationToken = default)
        {
            Guid mediaId = Guid.NewGuid();
            string storageKey = $"photos/{mediaId}{Path.GetExtension(fileName)}";

            MediaItem media = new() { MediaItemId = mediaId, Uri = storageKey, MediaType = (int)MediaItemType.Photo };
            await mediaItemLogic.Upsert(media, cancellationToken);

            await storageManager.Upsert(storageKey, content, cancellationToken);
            return mediaId;
        }

        public async Task<(Stream Stream, string ContentType)> GetImage(Guid mediaId, CancellationToken cancellationToken = default)
        {
            MediaItem media = await mediaItemLogic.Get(mediaId, cancellationToken) ?? throw new KeyNotFoundException($"Media {mediaId} not found.");

            Stream stream = await storageManager.Get(media.Uri, cancellationToken);
            return (stream, ContentTypeFor(media.Uri));
        }

        public async Task<Stream> GetThumbnail(Guid mediaId, int width, CancellationToken cancellationToken = default)
        {
            MediaItem media = await mediaItemLogic.Get(mediaId, cancellationToken) ?? throw new KeyNotFoundException($"Media {mediaId} not found.");

            await using Stream original = await storageManager.Get(media.Uri, cancellationToken);
            SKBitmap source = SKBitmap.Decode(original) ?? throw new InvalidOperationException($"Could not decode image for media {mediaId}.");
             
            int targetWidth = Math.Min(width, source.Width);
            int targetHeight = (int)Math.Round(source.Height * (targetWidth / (double)source.Width));

            SKImageInfo info = new(targetWidth, targetHeight);
            SKBitmap resized = source.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell))
                ?? throw new InvalidOperationException("Thumbnail resize failed.");
            using SKImage image = SKImage.FromBitmap(resized);
            using SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 80);

            MemoryStream output = new();
            data.SaveTo(output);
            output.Position = 0;
            return output;
        }

        private static string ContentTypeFor(string key) => Path.GetExtension(key).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream",
        };
    }
}
