using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.ServiceLogic;
using pzellhorn.Core.Messaging;

namespace PhotoProcessor.API.BackgroundServices
{
    public class UploadEventConsumer(
        IQueueConsumer queueConsumer,
        IServiceScopeFactory scopeFactory,
        UploadEventOptions options,
        ILogger<UploadEventConsumer> logger) : BackgroundService
    {
        private readonly UploadEventOptions _options = options;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using IAsyncDisposable subscription = await queueConsumer.Subscribe<S3EventNotification>(
                _options.Queue,
                _options.Exchange,
                _options.ExchangeType,
                _options.RoutingKey,
                HandleAsync,
                stoppingToken);

            logger.LogInformation($"Listening for upload events on queue '{_options.Queue}' bound to exchange '{_options.Exchange}'");

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            { 
            }
        }

        private async Task HandleAsync(S3EventNotification notification, CancellationToken cancellationToken)
        {
            foreach (S3EventRecord record in notification.Records)
            {
                string key = Uri.UnescapeDataString(record.S3.Object.Key);

                if (!TryGetMediaId(key, out Guid mediaId))
                {
                    logger.LogWarning("Ignoring upload event for unrecognised key '{Key}'", key);
                    continue;
                }
                 
                try
                {
                    await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                    IPhotoLogic photoLogic = scope.ServiceProvider.GetRequiredService<IPhotoLogic>();

                    Guid jobId = await photoLogic.EnqueueProcessing(mediaId, _options.JobType, cancellationToken);
                    logger.LogInformation($"Enqueued {_options.JobType} job {jobId} for media {mediaId} from upload event");
                }
                catch (KeyNotFoundException ex)
                {
                    logger.LogWarning(ex, "No media record for {MediaId}; dropping upload event", mediaId);
                }
            }
        }

        private bool TryGetMediaId(string key, out Guid mediaId)
        {
            mediaId = Guid.Empty;
            if (!key.StartsWith(_options.KeyPrefix, StringComparison.OrdinalIgnoreCase))
                return false;

            return Guid.TryParse(Path.GetFileNameWithoutExtension(key), out mediaId);
        }
    }
}
