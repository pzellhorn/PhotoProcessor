using Microsoft.Extensions.Logging;
using PhotoProcessor.DTO.enums;
using pzellhorn.Core.Logic.Base;
using pzellhorn.Core.State.Base.Interfaces;
using PhotoProcessor.State.Data.Entities;

namespace PhotoProcessor.Logic.EntityLogic
{
    public class JobLogic(IBaseRepository<Job> jobRepository, ILogger<JobLogic> logger) : BaseLogic<Job>(jobRepository)
    {
        public async Task MarkRunning(Guid jobId, CancellationToken cancellationToken = default)
        {
            Job job = await Get(jobId, cancellationToken) ?? throw new KeyNotFoundException($"Job {jobId} not found.");
            job.Status = (int)JobStatus.Running;
            await Upsert(job, cancellationToken);
        }

        public async Task MarkFailed(Guid jobId, string? error, CancellationToken cancellationToken = default)
        {
            Job job = await Get(jobId, cancellationToken) ?? throw new KeyNotFoundException($"Job {jobId} not found.");
            job.Status = (int)JobStatus.Failed;
            job.Error = error;
            await Upsert(job, cancellationToken);

            logger.LogWarning("Job {JobId} marked failed: {Error}", jobId, error ?? "(no detail)");
        }
         
    }
}
