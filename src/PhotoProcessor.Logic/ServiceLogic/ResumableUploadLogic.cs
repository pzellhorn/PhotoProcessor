using System.Security.Cryptography;
using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.State.Storage;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IResumableUploadLogic
    {
        Task<CreateUploadResponse> Create(CreateUploadRequest request, CancellationToken cancellationToken = default);
        Task<UploadStatusResponse> AppendChunk(Guid uploadId, long offset, Stream content, CancellationToken cancellationToken = default);
        Task<UploadStatusResponse> GetStatus(Guid uploadId, CancellationToken cancellationToken = default);
        Task Abort(Guid uploadId, CancellationToken cancellationToken = default);
    }

    public class ResumableUploadLogic(
        IMultipartStorage multipartStorage,
        IStorageManager storageManager,
        IMediaIngestLogic mediaIngestLogic,
        UploadSessionLogic uploadSessionLogic) : IResumableUploadLogic
    {
        private const int PreferredChunkSize = 8 * 1024 * 1024;

        private int ChunkSize => Math.Max(PreferredChunkSize, multipartStorage.MinChunkSize);

        public async Task<CreateUploadResponse> Create(CreateUploadRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.FileName))
                throw new ArgumentException("A file name is required.", nameof(request));
            if (request.TotalBytes <= 0)
                throw new ArgumentException("Total bytes must be greater than zero.", nameof(request));

            MediaItemType mediaType = GetMediaType(request.FileName);

            if (!string.IsNullOrWhiteSpace(request.ContentHash)
                && await mediaIngestLogic.FindByHash(request.ContentHash.ToLowerInvariant(), cancellationToken) is Guid existingId)
            {
                return new CreateUploadResponse { Duplicate = true, MediaId = existingId, ChunkSize = ChunkSize };
            }

            Guid uploadId = Guid.NewGuid();
            string stagingKey = $"uploads/{uploadId}{Path.GetExtension(request.FileName)}";
            string multipartUploadId = await multipartStorage.BeginMultipart(stagingKey, cancellationToken);

            UploadSession session = new()
            {
                UploadSessionId = uploadId,
                FileName = request.FileName,
                StagingKey = stagingKey,
                MultipartUploadId = multipartUploadId,
                MediaType = (int)mediaType,
                TotalBytes = request.TotalBytes,
                ReceivedBytes = 0,
                NextPartNumber = 1,
            };
            await uploadSessionLogic.Upsert(session, cancellationToken);

            return new CreateUploadResponse { UploadId = uploadId, ChunkSize = ChunkSize, ReceivedBytes = 0 };
        }

        public async Task<UploadStatusResponse> AppendChunk(Guid uploadId, long offset, Stream content, CancellationToken cancellationToken = default)
        {
            UploadSession session = await Load(uploadId, cancellationToken);

            if (session.Completed)
                return ToStatus(session);

            if (offset < session.ReceivedBytes)
                return ToStatus(session);

            if (offset > session.ReceivedBytes)
                throw new ArgumentException($"Chunk starts at {offset} but the next expected byte is {session.ReceivedBytes}.", nameof(offset));

            using MemoryStream buffer = new();
            await content.CopyToAsync(buffer, cancellationToken);
            buffer.Position = 0;

            long length = buffer.Length;
            if (length == 0)
                throw new ArgumentException("Chunk was empty.", nameof(content));

            bool isFinalChunk = session.ReceivedBytes + length == session.TotalBytes;
            if (!isFinalChunk && length < multipartStorage.MinChunkSize)
                throw new ArgumentException($"Chunks must be at least {multipartStorage.MinChunkSize} bytes unless they complete the upload.", nameof(content));

            await multipartStorage.UploadPart(session.StagingKey, session.MultipartUploadId, session.NextPartNumber, buffer, cancellationToken);

            session.ReceivedBytes += length;
            session.NextPartNumber++;
            await uploadSessionLogic.Upsert(session, cancellationToken);

            if (session.ReceivedBytes == session.TotalBytes)
                await Finalise(session, cancellationToken);

            return ToStatus(session);
        }

        public async Task<UploadStatusResponse> GetStatus(Guid uploadId, CancellationToken cancellationToken = default)
        {
            return ToStatus(await Load(uploadId, cancellationToken));
        }

        public async Task Abort(Guid uploadId, CancellationToken cancellationToken = default)
        {
            UploadSession session = await Load(uploadId, cancellationToken);

            if (!session.Completed)
                await multipartStorage.AbortMultipart(session.StagingKey, session.MultipartUploadId, cancellationToken);

            await uploadSessionLogic.Delete(uploadId, cancellationToken);
        }

        private async Task Finalise(UploadSession session, CancellationToken cancellationToken)
        {
            await multipartStorage.CompleteMultipart(session.StagingKey, session.MultipartUploadId, cancellationToken);

            string contentHash;
            await using (Stream assembled = await storageManager.Get(session.StagingKey, cancellationToken))
                contentHash = Convert.ToHexString(await SHA256.HashDataAsync(assembled, cancellationToken)).ToLowerInvariant();

            (Guid mediaId, bool duplicate) = await mediaIngestLogic.Register(
                session.FileName,
                contentHash,
                session.StagingKey,
                (MediaItemType)session.MediaType,
                cancellationToken);

            session.Completed = true;
            session.MediaId = mediaId;
            session.Duplicate = duplicate;
            await uploadSessionLogic.Upsert(session, cancellationToken);
        }

        private async Task<UploadSession> Load(Guid uploadId, CancellationToken cancellationToken)
            => await uploadSessionLogic.Get(uploadId, cancellationToken) ?? throw new KeyNotFoundException($"Upload session {uploadId} not found.");

        private UploadStatusResponse ToStatus(UploadSession session) => new()
        {
            UploadId = session.UploadSessionId,
            TotalBytes = session.TotalBytes,
            ReceivedBytes = session.ReceivedBytes,
            ChunkSize = ChunkSize,
            Complete = session.Completed,
            Duplicate = session.Duplicate,
            MediaId = session.MediaId,
            MediaType = (MediaItemType)session.MediaType,
        };

        private static MediaItemType GetMediaType(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" => MediaItemType.Photo,
            ".mp4" or ".mov" or ".mkv" or ".avi" or ".webm" or ".m4v" => MediaItemType.Video,
            _ => throw new ArgumentException($"Unsupported file type '{Path.GetExtension(fileName)}'.", nameof(fileName)),
        };
    }
}
