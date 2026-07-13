using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.State.Storage;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IVideoLogic
    {
        Task Submit(SubmitVideoRequest request, CancellationToken cancellationToken = default);
        Task DeleteRenditions(Guid mediaId, CancellationToken cancellationToken = default);
        Task<(Stream Stream, string ContentType)> GetDerivedAsset(Guid mediaId, string assetPath, CancellationToken cancellationToken = default);
        Task<List<VideoRenditionSummary>> GetRenditions(Guid mediaId, CancellationToken cancellationToken = default);
    }

    public class VideoLogic(JobLogic jobLogic, MediaItemLogic mediaItemLogic, VideoRenditionLogic videoRenditionLogic, IStorageManager storageManager) : IVideoLogic
    {
        public async Task Submit(SubmitVideoRequest request, CancellationToken cancellationToken = default)
        {
            Job job = await jobLogic.Get(request.JobId, cancellationToken) ?? throw new KeyNotFoundException($"Job {request.JobId} not found.");
            Guid mediaId = job.MediaId;

            MediaItem media = await mediaItemLogic.Get(mediaId, cancellationToken) ?? throw new KeyNotFoundException($"Media {mediaId} not found.");
            media.DurationMs = request.DurationMs;
            media.ThumbnailUri = request.PosterPath;
            await mediaItemLogic.Upsert(media, cancellationToken);

            List<VideoRendition> existing = await videoRenditionLogic.GetFor(mediaId, r => r.MediaId, cancellationToken);
            foreach (VideoRendition rendition in existing)
                await videoRenditionLogic.Delete(rendition.RenditionId, cancellationToken);

            foreach (VideoRenditionDto dto in request.Renditions)
            {
                VideoRendition rendition = new()
                {
                    RenditionId = Guid.NewGuid(),
                    MediaId = mediaId,
                    Format = dto.Format,
                    EntryPath = dto.EntryPath,
                    Width = dto.Width,
                    Height = dto.Height,
                    Bitrate = dto.Bitrate,
                };
                await videoRenditionLogic.Upsert(rendition, cancellationToken);
            }

            job.Status = (int)JobStatus.Done;
            await jobLogic.Upsert(job, cancellationToken);
        }

        public async Task DeleteRenditions(Guid mediaId, CancellationToken cancellationToken = default)
        {
            await storageManager.DeleteByPrefix($"videos/derived/{mediaId}/", cancellationToken);

            List<VideoRendition> renditions = await videoRenditionLogic.GetFor(mediaId, r => r.MediaId, cancellationToken);
            foreach (VideoRendition rendition in renditions)
                await videoRenditionLogic.Delete(rendition.RenditionId, cancellationToken);
        }

        public async Task<(Stream Stream, string ContentType)> GetDerivedAsset(Guid mediaId, string assetPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(assetPath) || assetPath.Contains(".."))   // a path containing ".." denotes a relative pathing
                throw new ArgumentException("Invalid asset path.", nameof(assetPath));

            string key = $"videos/derived/{mediaId}/{assetPath}";
            Stream stream = await storageManager.Get(key, cancellationToken);
            return (stream, ContentTypeFor(assetPath));
        }

        public async Task<List<VideoRenditionSummary>> GetRenditions(Guid mediaId, CancellationToken cancellationToken = default)
        {
            List<VideoRendition> renditions = await videoRenditionLogic.GetFor(mediaId, r => r.MediaId, cancellationToken);
            string prefix = $"videos/derived/{mediaId}/";

            List<VideoRenditionSummary> summaries = new();
            foreach (VideoRendition rendition in renditions)
            {
                summaries.Add(new VideoRenditionSummary
                {
                    Format = rendition.Format,
                    AssetPath = rendition.EntryPath.StartsWith(prefix) ? rendition.EntryPath[prefix.Length..] : rendition.EntryPath,
                    Width = rendition.Width,
                    Height = rendition.Height,
                    Bitrate = rendition.Bitrate,
                });
            }
            return summaries;
        }

        private static string ContentTypeFor(string path) => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".m3u8" => "application/vnd.apple.mpegurl",
            ".ts" => "video/mp2t",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".mp4" => "video/mp4",
            ".vtt" => "text/vtt",
            _ => "application/octet-stream",
        };
    }
}
