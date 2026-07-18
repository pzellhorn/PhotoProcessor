using Microsoft.Extensions.Options;
using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.Scaling;
using PhotoProcessor.State.Data.Queries;
using pzellhorn.Core.Messaging;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IProgressLogic
    {
        Task<List<JobTypeProgress>> GetProgress(CancellationToken cancellationToken = default);
        Task<int> Backfill(JobTypes jobType, CancellationToken cancellationToken = default);
        Task<JobTypeProgress> Scale(JobTypes jobType, int replicas, CancellationToken cancellationToken = default);
    }

    public class ProgressLogic(
        IProgressQueries progressQueries,
        IQueueInspector queueInspector,
        IMediaLogic mediaLogic,
        IWorkerScaler workerScaler,
        IOptions<WorkerScalingOptions> scalingOptions) : IProgressLogic
    {
        private readonly WorkerScalingOptions _scalingOptions = scalingOptions.Value;

        private static List<int> ToInts(List<MediaItemType> mediaTypes)
        {
            List<int> values = new();
            foreach (MediaItemType mediaType in mediaTypes)
                values.Add((int)mediaType);
            return values;
        }
        public async Task<List<JobTypeProgress>> GetProgress(CancellationToken cancellationToken = default)
        {
            List<JobTypeProgress> progress = new();

            foreach (JobTypes jobType in JobMediaTypes.All())
            {
                List<MediaItemType> mediaTypes = JobMediaTypes.GetMediaTypesForJob(jobType);
                string queue = JobQueues.GetQueueForJob(jobType);

                JobTypeCounts counts = await progressQueries.CountsFor(
                    (int)jobType,
                    ToInts(mediaTypes),
                    (int)JobStatus.Done,
                    (int)JobStatus.Queued,
                    (int)JobStatus.Running,
                    (int)JobStatus.Failed,
                    cancellationToken);

                QueueDepth depth = await queueInspector.GetDepth(queue, cancellationToken);

                string deployment = _scalingOptions.Deployments.GetValueOrDefault(jobType, string.Empty);
                WorkerScale scale = string.IsNullOrEmpty(deployment)
                    ? new WorkerScale(string.Empty, 0, 0, false)
                    : await workerScaler.GetScale(deployment, cancellationToken);

                progress.Add(new JobTypeProgress
                {
                    Deployment = deployment,
                    ScalingEnabled = workerScaler.Enabled && !string.IsNullOrEmpty(deployment),
                    DeploymentFound = scale.Found,
                    Replicas = scale.Replicas,
                    ReadyReplicas = scale.ReadyReplicas,
                    MaxReplicas = workerScaler.MaxReplicas,
                    JobType = jobType,
                    AppliesTo = mediaTypes,
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
            List<MediaItemType> mediaTypes = JobMediaTypes.GetMediaTypesForJob(jobType);

            List<Guid> mediaIds = await progressQueries.MediaNeverRun((int)jobType, ToInts(mediaTypes), cancellationToken);

            int enqueued = 0;
            foreach (Guid mediaId in mediaIds)
            {
                await mediaLogic.EnqueueProcessing(mediaId, jobType, cancellationToken);
                enqueued++;
            }

            return enqueued;
        }

        public async Task<JobTypeProgress> Scale(JobTypes jobType, int replicas, CancellationToken cancellationToken = default)
        {
            if (!_scalingOptions.Deployments.TryGetValue(jobType, out string? deployment))
                throw new KeyNotFoundException($"No worker deployment configured for {jobType}.");

            await workerScaler.SetScale(deployment, replicas, cancellationToken);

            List<JobTypeProgress> progress = await GetProgress(cancellationToken);
            return progress.Single(p => p.JobType == jobType);
        }
    }
}
