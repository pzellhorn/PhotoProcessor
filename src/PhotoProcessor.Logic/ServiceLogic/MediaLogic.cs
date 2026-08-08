using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using PhotoProcessor.Logic.Observability;
using PhotoProcessor.State.Data.Queries;
using pzellhorn.Core.Messaging;
using pzellhorn.Core.State.Storage;
using SkiaSharp;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IMediaLogic
    {
        Task<Guid> EnqueueProcessing(Guid mediaId, JobTypes jobType, CancellationToken cancellationToken = default);
        Task<MediaLibraryPage> ListLibrary(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Uri> GetDownloadUrl(Guid mediaId, CancellationToken cancellationToken = default);

        Task<(Stream Stream, string ContentType)> GetImage(Guid mediaId, CancellationToken cancellationToken = default);
        Task<Stream> GetThumbnail(Guid mediaId, int width, CancellationToken cancellationToken = default);
        Task<Stream> GetFaceThumbnail(Guid fingerprintId, int width, CancellationToken cancellationToken = default);
        Task Delete(Guid mediaId, CancellationToken cancellationToken = default);
    }

    public class MediaLogic(ISignedUrlProvider signedUrlProvider, IStorageManager storageManager, IQueuePublisher queuePublisher, JobLogic jobLogic, MediaItemLogic mediaItemLogic, FingerprintLogic fingerprintLogic, ImageEmbeddingLogic imageEmbeddingLogic, TagLogic tagLogic, IVideoLogic videoLogic, IMediaQueries mediaQueries, PipelineMetrics metrics) : IMediaLogic
    {
        private static readonly TimeSpan UrlLifetime = TimeSpan.FromMinutes(30);

        private const double FaceCropMargin = 0.25;

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
            await queuePublisher.Publish(JobQueues.GetQueueForJob(jobType), message, cancellationToken);
            metrics.JobEnqueued(jobType);

            return jobId;
        } 

        public async Task<MediaLibraryPage> ListLibrary(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            (List<MediaItem> items, int totalCount) = await mediaQueries.ListLibrary(page, pageSize, cancellationToken);

            MediaLibraryPage result = new() { TotalCount = totalCount };
            foreach (MediaItem media in items)
            {
                result.Items.Add(new MediaLibraryItem
                {
                    MediaItemId = media.MediaItemId,
                    MediaType = (MediaItemType)media.MediaType,
                    DurationMs = media.DurationMs,
                });
            }
            return result;
        }

        public async Task<Uri> GetDownloadUrl(Guid mediaId, CancellationToken cancellationToken = default)
        {
            MediaItem media = await mediaItemLogic.Get(mediaId, cancellationToken) ?? throw new KeyNotFoundException($"Media {mediaId} not found.");

            return await signedUrlProvider.GetDownloadUrl(media.Uri, UrlLifetime, cancellationToken);
        }
 
        public async Task<(Stream Stream, string ContentType)> GetImage(Guid mediaId, CancellationToken cancellationToken = default)
        {
            MediaItem media = await mediaItemLogic.Get(mediaId, cancellationToken) ?? throw new KeyNotFoundException($"Media {mediaId} not found.");

            Stream stream = await storageManager.Get(media.Uri, cancellationToken);
            return (stream, GetContentType(media.Uri));
        }

        public async Task<Stream> GetThumbnail(Guid mediaId, int width, CancellationToken cancellationToken = default)
        {
            MediaItem media = await mediaItemLogic.Get(mediaId, cancellationToken) ?? throw new KeyNotFoundException($"Media {mediaId} not found.");

            string sourceKey = string.IsNullOrEmpty(media.ThumbnailUri) ? media.Uri : media.ThumbnailUri;

            await using Stream original = await storageManager.Get(sourceKey, cancellationToken);
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

        public async Task<Stream> GetFaceThumbnail(Guid fingerprintId, int width, CancellationToken cancellationToken = default)
        {
            Fingerprint fingerprint = await fingerprintLogic.Get(fingerprintId, cancellationToken) ?? throw new KeyNotFoundException($"Fingerprint {fingerprintId} not found.");

            if (fingerprint.BoundingX is not double boxX || fingerprint.BoundingY is not double boxY ||
                fingerprint.BoundingWidth is not double boxWidth || fingerprint.BoundingHeight is not double boxHeight)
                throw new InvalidOperationException($"Fingerprint {fingerprintId} has no bounding box.");

            MediaItem media = await mediaItemLogic.Get(fingerprint.MediaId, cancellationToken) ?? throw new KeyNotFoundException($"Media {fingerprint.MediaId} not found.");

            await using Stream original = await storageManager.Get(media.Uri, cancellationToken);
            using SKBitmap source = SKBitmap.Decode(original) ?? throw new InvalidOperationException($"Could not decode image for media {fingerprint.MediaId}.");

            double marginX = boxWidth * FaceCropMargin;
            double marginY = boxHeight * FaceCropMargin;
            float left = (float)Math.Max(0, boxX - marginX);
            float top = (float)Math.Max(0, boxY - marginY);
            float right = (float)Math.Min(source.Width, boxX + boxWidth + marginX);
            float bottom = (float)Math.Min(source.Height, boxY + boxHeight + marginY);

            if (right <= left || bottom <= top)
                throw new InvalidOperationException($"Fingerprint {fingerprintId} bounding box lies outside the image.");

            SKRect cropRect = new(left, top, right, bottom);

            int targetWidth = Math.Max(1, Math.Min(width, (int)cropRect.Width));
            int targetHeight = Math.Max(1, (int)Math.Round(cropRect.Height * (targetWidth / (double)cropRect.Width)));

            SKImageInfo info = new(targetWidth, targetHeight);
            using SKSurface surface = SKSurface.Create(info);
            using SKImage sourceImage = SKImage.FromBitmap(source);
            surface.Canvas.DrawImage(sourceImage, cropRect, new SKRect(0, 0, targetWidth, targetHeight), new SKSamplingOptions(SKCubicResampler.Mitchell));

            using SKImage face = surface.Snapshot();
            using SKData data = face.Encode(SKEncodedImageFormat.Jpeg, 80);

            MemoryStream output = new();
            data.SaveTo(output);
            output.Position = 0;
            return output;
        }
         
        public async Task Delete(Guid mediaId, CancellationToken cancellationToken = default)
        {
            MediaItem media = await mediaItemLogic.Get(mediaId, cancellationToken) ?? throw new KeyNotFoundException($"Media {mediaId} not found.");

            await storageManager.Delete(media.Uri, cancellationToken);

            if (media.MediaType == (int)MediaItemType.Video)
            {
                await videoLogic.DeleteRenditions(mediaId, cancellationToken);

                List<MediaItem> frames = await mediaItemLogic.GetFor<Guid?>(mediaId, m => m.ParentMediaId, cancellationToken);
                foreach (MediaItem frame in frames)
                    await Delete(frame.MediaItemId, cancellationToken);
            }

            List<ImageEmbedding> imageEmbeddings = await imageEmbeddingLogic.GetFor(mediaId, e => e.MediaId, cancellationToken);
            foreach (ImageEmbedding imageEmbedding in imageEmbeddings)
                await imageEmbeddingLogic.Delete(imageEmbedding.ImageEmbeddingId, cancellationToken);

            List<Fingerprint> fingerprints = await fingerprintLogic.GetFor(mediaId, f => f.MediaId, cancellationToken);
            HashSet<Guid> affectedTagIds = new();
            foreach (Fingerprint fingerprint in fingerprints)
            {
                if (fingerprint.TagId is Guid tagId)
                    affectedTagIds.Add(tagId);
                await fingerprintLogic.Delete(fingerprint.FingerprintId, cancellationToken);
            }
             
            //Check tags to see if now no more mapped fingerprints.
            foreach (Guid tagId in affectedTagIds)
            {
                List<Fingerprint> remaining = await fingerprintLogic.GetFor(tagId, f => f.TagId, cancellationToken);
                if (remaining.Count == 0)
                    await tagLogic.Delete(tagId, cancellationToken);
            }

            await mediaItemLogic.Delete(mediaId, cancellationToken);
        }

        private static string GetContentType(string key) => Path.GetExtension(key).ToLowerInvariant() switch
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
