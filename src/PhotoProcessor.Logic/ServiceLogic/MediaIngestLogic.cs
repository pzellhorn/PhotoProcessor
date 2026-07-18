using System.Security.Cryptography;
using PhotoProcessor.DTO.enums;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.State.Storage;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IMediaIngestLogic
    {
        Task<(Guid MediaId, bool Duplicate)> Store(string fileName, Stream content, MediaItemType mediaType, CancellationToken cancellationToken = default);

        Task<Guid?> FindByHash(string contentHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publishes an object that is already in storage under a staging key: dedupes on hash, creates the
        /// MediaItem, then moves the object into the watched prefix so the bucket event finds a record waiting.
        /// </summary>
        Task<(Guid MediaId, bool Duplicate)> Register(string fileName, string contentHash, string stagingKey, MediaItemType mediaType, CancellationToken cancellationToken = default);
    }

    public class MediaIngestLogic(IStorageManager storageManager, MediaItemLogic mediaItemLogic) : IMediaIngestLogic
    {
        public async Task<(Guid MediaId, bool Duplicate)> Store(string fileName, Stream content, MediaItemType mediaType, CancellationToken cancellationToken = default)
        {
            string contentHash = Convert.ToHexString(await SHA256.HashDataAsync(content, cancellationToken)).ToLowerInvariant();
            content.Position = 0;

            List<MediaItem> existing = await mediaItemLogic.GetFor(contentHash, m => m.ContentHash, cancellationToken);
            if (existing.Count > 0)
                return (existing[0].MediaItemId, true);

            Guid mediaId = Guid.NewGuid();
            string storageKey = $"{GetStoragePrefix(mediaType)}{mediaId}{Path.GetExtension(fileName)}";

            MediaItem media = new() { MediaItemId = mediaId, Uri = storageKey, MediaType = (int)mediaType, ContentHash = contentHash };
            await mediaItemLogic.Upsert(media, cancellationToken);

            await storageManager.Upsert(storageKey, content, cancellationToken);
            return (mediaId, false);
        }

        public async Task<Guid?> FindByHash(string contentHash, CancellationToken cancellationToken = default)
        {
            List<MediaItem> existing = await mediaItemLogic.GetFor(contentHash, m => m.ContentHash, cancellationToken);
            return existing.Count > 0 ? existing[0].MediaItemId : null;
        }

        public async Task<(Guid MediaId, bool Duplicate)> Register(string fileName, string contentHash, string stagingKey, MediaItemType mediaType, CancellationToken cancellationToken = default)
        {
            if (await FindByHash(contentHash, cancellationToken) is Guid duplicateId)
            {
                await storageManager.Delete(stagingKey, cancellationToken);
                return (duplicateId, true);
            }

            Guid mediaId = Guid.NewGuid();
            string storageKey = $"{GetStoragePrefix(mediaType)}{mediaId}{Path.GetExtension(fileName)}";

            MediaItem media = new() { MediaItemId = mediaId, Uri = storageKey, MediaType = (int)mediaType, ContentHash = contentHash };
            await mediaItemLogic.Upsert(media, cancellationToken);

            await storageManager.Move(stagingKey, storageKey, cancellationToken);
            return (mediaId, false);
        }

        private static string GetStoragePrefix(MediaItemType mediaType)
        {
            return mediaType switch
            {
                MediaItemType.Photo => "photos/",
                MediaItemType.Video => "videos/raw/",
                _ => throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, "No storage prefix for media type."),
            };
        }
    }
}
