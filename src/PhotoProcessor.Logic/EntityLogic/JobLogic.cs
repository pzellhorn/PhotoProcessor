using Microsoft.Extensions.Logging;
using PhotoProcessor.DTO.enums;
using pzellhorn.Core.Logic.Base;
using pzellhorn.Core.State.Base.Interfaces;
using PhotoProcessor.Logic.Observability;
using PhotoProcessor.State.Data.Entities;

namespace PhotoProcessor.Logic.EntityLogic
{
    public class JobLogic(IBaseRepository<Job> jobRepository, PipelineMetrics metrics, ILogger<JobLogic> logger) : BaseLogic<Job>(jobRepository)
    {
        public async Task MarkRunning(Guid jobId, CancellationToken cancellationToken = default)
        {
            Job job = await Get(jobId, cancellationToken) ?? throw new KeyNotFoundException($"Job {jobId} not found.");
            metrics.JobStarted((JobTypes)job.JobType, DateTime.UtcNow - job.CreatedAt);

            job.Status = (int)JobStatus.Running;
            await Upsert(job, cancellationToken);
        }

        public async Task MarkFailed(Guid jobId, string? error, CancellationToken cancellationToken = default)
        {
            Job job = await Get(jobId, cancellationToken) ?? throw new KeyNotFoundException($"Job {jobId} not found.");

            RecordFinished(job, JobStatus.Failed);

            job.Status = (int)JobStatus.Failed;
            job.Error = error;
            await Upsert(job, cancellationToken);

            logger.LogWarning("Job {JobId} marked failed: {Error}", jobId, error ?? "(no detail)");
        }

        public async Task MarkDone(Job job, CancellationToken cancellationToken = default)
        {
            RecordFinished(job, JobStatus.Done);

            job.Status = (int)JobStatus.Done;
            await Upsert(job, cancellationToken);
        }

        private void RecordFinished(Job job, JobStatus outcome)
        {
            DateTime startedAt = job.Status == (int)JobStatus.Running ? job.ModifiedAt : job.CreatedAt;
            metrics.JobFinished((JobTypes)job.JobType, outcome, DateTime.UtcNow - startedAt);
        }
    }
}
