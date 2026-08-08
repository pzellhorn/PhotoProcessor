using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IKeyframeLogic
    {
        Task<int> Submit(SubmitKeyframesRequest request, CancellationToken cancellationToken = default);
    }

    public class KeyframeLogic(JobLogic jobLogic, MediaItemLogic mediaItemLogic, IMediaLogic mediaLogic) : IKeyframeLogic
    {
        public async Task<int> Submit(SubmitKeyframesRequest request, CancellationToken cancellationToken = default)
        {
            Job job = await jobLogic.Get(request.JobId, cancellationToken) ?? throw new KeyNotFoundException($"Job {request.JobId} not found.");
            Guid videoId = job.MediaId;

            _ = await mediaItemLogic.Get(videoId, cancellationToken) ?? throw new KeyNotFoundException($"Media {videoId} not found.");

            List<MediaItem> existing = await mediaItemLogic.GetFor<Guid?>(videoId, m => m.ParentMediaId, cancellationToken);
            foreach (MediaItem frame in existing)
                await mediaLogic.Delete(frame.MediaItemId, cancellationToken);

            int created = 0;
            foreach (KeyframeDto keyframe in request.Frames)
            {
                Guid frameId = Guid.NewGuid();

                MediaItem frame = new()
                {
                    MediaItemId = frameId,
                    MediaType = (int)MediaItemType.Frame,
                    Uri = keyframe.StoragePath, 
                    ContentHash = string.Empty,
                    ParentMediaId = videoId,
                    TimestampMs = keyframe.TimestampMs,
                };
                await mediaItemLogic.Upsert(frame, cancellationToken);

                foreach (JobTypes imageJob in JobMediaTypes.ImageJobs())
                    await mediaLogic.EnqueueProcessing(frameId, imageJob, cancellationToken);

                created++;
            }

            await jobLogic.MarkDone(job, cancellationToken);

            return created;
        }
    }
}
