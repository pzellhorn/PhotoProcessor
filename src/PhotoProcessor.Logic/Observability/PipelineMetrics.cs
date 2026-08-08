using System.Diagnostics.Metrics;
using PhotoProcessor.DTO.enums;

namespace PhotoProcessor.Logic.Observability
{
    /// <summary>
    /// Metrics we expose via Prometheus scrape
    /// </summary>
    public class PipelineMetrics : IDisposable
    {
        public const string MeterName = "PhotoProcessor.Pipeline";

        private readonly Meter _meter;

        private readonly Counter<long> _enqueued;
        private readonly Counter<long> _finished;
        private readonly Histogram<double> _queueWaitSeconds;
        private readonly Histogram<double> _processingSeconds;
        private readonly Histogram<double> _searchSeconds;
        private readonly Histogram<double> _textEncodeSeconds;
        private readonly Counter<long> _searchFailures;

        public PipelineMetrics(IMeterFactory meterFactory)
        {
            _meter = meterFactory.Create(MeterName);

            _enqueued = _meter.CreateCounter<long>(
                "photoprocessor.jobs.enqueued", "{job}", "Jobs published onto a worker queue.");

            _finished = _meter.CreateCounter<long>(
                "photoprocessor.jobs.finished", "{job}", "Jobs that reached a terminal state.");

            _queueWaitSeconds = _meter.CreateHistogram<double>(
                "photoprocessor.jobs.queue_wait", "s", "Time from a job being enqueued to a worker picking it up.");

            _processingSeconds = _meter.CreateHistogram<double>(
                "photoprocessor.jobs.processing", "s", "Time a worker spent on a job.");

            _searchSeconds = _meter.CreateHistogram<double>(
                "photoprocessor.search.duration", "s", "End to end semantic search latency.");

            _textEncodeSeconds = _meter.CreateHistogram<double>(
                "photoprocessor.textencoder.duration", "s", "Latency of the synchronous CLIP text encode call.");

            _searchFailures = _meter.CreateCounter<long>(
                "photoprocessor.search.failures", "{failure}", "Searches that could not be served.");
        }

        public void JobEnqueued(JobTypes jobType) => _enqueued.Add(1, Tag(jobType));

        public void JobStarted(JobTypes jobType, TimeSpan waited) => _queueWaitSeconds.Record(waited.TotalSeconds, Tag(jobType));

        public void JobFinished(JobTypes jobType, JobStatus outcome, TimeSpan processing)
        {
            KeyValuePair<string, object?>[] tags =
            [
                new("job_type", jobType.ToString()),
                new("outcome", outcome.ToString()),
            ];

            _finished.Add(1, tags);
            _processingSeconds.Record(processing.TotalSeconds, tags);
        }

        public void SearchCompleted(TimeSpan elapsed) => _searchSeconds.Record(elapsed.TotalSeconds);

        public void SearchFailed(string reason) =>_searchFailures.Add(1, new KeyValuePair<string, object?>("reason", reason));

        public void TextEncoded(TimeSpan elapsed) => _textEncodeSeconds.Record(elapsed.TotalSeconds);

        private static KeyValuePair<string, object?>[] Tag(JobTypes jobType) => [new("job_type", jobType.ToString())];

        public void Dispose() => _meter.Dispose();
    }
}
