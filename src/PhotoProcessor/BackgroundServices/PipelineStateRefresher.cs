using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.Observability;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.BackgroundServices
{
 /// <summary>
 /// Updates state of pipelines
 /// </summary>
 /// <param name="scopeFactory"></param>
 /// <param name="state"></param>
 /// <param name="logger"></param>
    public class PipelineStateRefresher(
        IServiceScopeFactory scopeFactory,
        PipelineState state,
        ILogger<PipelineStateRefresher> logger) : BackgroundService
    {
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(15);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new(RefreshInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                    IProgressLogic progressLogic = scope.ServiceProvider.GetRequiredService<IProgressLogic>();

                    List<JobTypeProgress> progress = await progressLogic.GetProgress(stoppingToken);
                    state.Update(progress);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Could not refresh pipeline metrics snapshot");
                }

                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
