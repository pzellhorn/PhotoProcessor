using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.State.Data.Queries;
using pzellhorn.Core.Messaging;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IProgressLogic
    {
        Task<List<JobTypeProgress>> GetProgress(CancellationToken cancellationToken = default);
        Task<int> Backfill(JobTypes jobType, CancellationToken cancellationToken = default);
    }

    public class ProgressLogic(IProgressQueries progressQueries, IQueueInspector queueInspector, IMediaLogic mediaLogic) : IProgressLogic
    {
        public async Task<List<JobTypeProgress>> GetProgress(CancellationToken cancellationToken = default)
        {
            List<JobTypeProgress> progress = new();

            foreach (JobTypes jobType in JobMediaTypes.All())
            {
                MediaItemType mediaType = JobMediaTypes.GetMediaTypeForJob(jobType);
                string queue = JobQueues.GetQueueForJob(jobType);

                JobTypeCounts counts = await progressQueries.CountsFor(
                    (int)jobType,
                    (int)mediaType,
                    (int)JobStatus.Done,
                    (int)JobStatus.Queued,
                    (int)JobStatus.Running,
                    (int)JobStatus.Failed,
                    cancellationToken);

                QueueDepth depth = await queueInspector.GetDepth(queue, cancellationToken);

                progress.Add(new JobTypeProgress
                {
                    JobType = jobType,
                    AppliesTo = mediaType,
                    Queue = queue,
                    QueueDepth = depth.MessageCount,
                    Consumers = depth.ConsumerCount,
                    QueueExists = depth.Exists,
                    EligibleMedia = counts.EligibleMedia,
                    Done = counts.Done,
                    Queued = counts.Queued,
                    Running = counts.Running,
                    Failed = counts.Failed,
                    NeverRun = counts.NeverRun,
                });
            }

            return progress;
        }

        public async Task<int> Backfill(JobTypes jobType, CancellationToken cancellationToken = default)
        {
            MediaItemType mediaType = JobMediaTypes.GetMediaTypeForJob(jobType);

            List<Guid> mediaIds = await progressQueries.MediaNeverRun((int)jobType, (int)mediaType, cancellationToken);

            int enqueued = 0;
            foreach (Guid mediaId in mediaIds)
            {
                await mediaLogic.EnqueueProcessing(mediaId, jobType, cancellationToken);
                enqueued++;
            }

            return enqueued;
        }
    }
}
