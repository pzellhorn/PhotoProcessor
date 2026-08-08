using System.Diagnostics.Metrics;
using PhotoProcessor.DTO.ServiceDTOs;

namespace PhotoProcessor.Logic.Observability
{
    /// <summary>
    /// Latest pipeline snapshot, refreshed on a timer. Feeds prometheus scrape data
    /// </summary>
    public class PipelineState
    {
        private volatile List<JobTypeProgress> _snapshot = [];

        public List<JobTypeProgress> Snapshot => _snapshot;

        public void Update(List<JobTypeProgress> progress) => _snapshot = progress;
    }

    public class PipelineStateMetrics : IDisposable
    {
        private readonly Meter _meter;

        public PipelineStateMetrics(IMeterFactory meterFactory, PipelineState state)
        {
            _meter = meterFactory.Create(PipelineMetrics.MeterName);

            _meter.CreateObservableGauge("photoprocessor.queue.depth", () => Observe(state, p => p.QueueDepth),
                "{message}", "Messages waiting on the job queue.");

            _meter.CreateObservableGauge("photoprocessor.queue.consumers", () => Observe(state, p => p.Consumers),
                "{consumer}", "Workers currently subscribed to the queue.");

            _meter.CreateObservableGauge("photoprocessor.media.eligible", () => Observe(state, p => p.EligibleMedia),
                "{item}", "Media items this job type applies to.");

            _meter.CreateObservableGauge("photoprocessor.jobs.done", () => Observe(state, p => p.Done),
                "{item}", "Media items with a completed job of this type.");

            _meter.CreateObservableGauge("photoprocessor.jobs.failed", () => Observe(state, p => p.Failed),
                "{item}", "Media items whose job of this type failed.");

            _meter.CreateObservableGauge("photoprocessor.jobs.never_run", () => Observe(state, p => p.NeverRun),
                "{item}", "Eligible media items never processed by this job type.");

            _meter.CreateObservableGauge("photoprocessor.jobs.coverage", () => Observe(state, Coverage),
                "1", "Fraction of eligible media processed by this job type.");

            _meter.CreateObservableGauge("photoprocessor.workers.replicas", () => Observe(state, p => p.Replicas),
                "{pod}", "Desired worker replicas.");

            _meter.CreateObservableGauge("photoprocessor.workers.ready", () => Observe(state, p => p.ReadyReplicas),
                "{pod}", "Ready worker replicas.");
        }

        private static double Coverage(JobTypeProgress progress) =>
            progress.EligibleMedia == 0 ? 1 : (double)progress.Done / progress.EligibleMedia;

        private static IEnumerable<Measurement<double>> Observe(PipelineState state, Func<JobTypeProgress, double> select)
        {
            List<Measurement<double>> measurements = new();

            foreach (JobTypeProgress progress in state.Snapshot)
            {
                measurements.Add(new Measurement<double>(select(progress),
                    new KeyValuePair<string, object?>("job_type", progress.JobType.ToString()),
                    new KeyValuePair<string, object?>("queue", progress.Queue)));
            }

            return measurements;
        }

        public void Dispose() => _meter.Dispose();
    }
}
