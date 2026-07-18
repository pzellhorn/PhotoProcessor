namespace PhotoProcessor.Logic.Scaling
{
    public record WorkerScale(string Deployment, int Replicas, int ReadyReplicas, bool Found);

    /// <summary>
    /// Reads and adjusts the replica count of a worker deployment.
    /// </summary>
    public interface IWorkerScaler
    {
        bool Enabled { get; }
        int MaxReplicas { get; }

        Task<WorkerScale> GetScale(string deployment, CancellationToken cancellationToken = default);
        Task<WorkerScale> SetScale(string deployment, int replicas, CancellationToken cancellationToken = default);
    }
}
