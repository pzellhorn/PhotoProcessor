using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Options;

namespace PhotoProcessor.Logic.Scaling
{
    public class KubernetesWorkerScaler : IWorkerScaler
    {
        private readonly WorkerScalingOptions _options;
        private readonly Lazy<IKubernetes> _client;

        public KubernetesWorkerScaler(IOptions<WorkerScalingOptions> options)
        {
            _options = options.Value;
            _client = new Lazy<IKubernetes>(() =>
                new Kubernetes(KubernetesClientConfiguration.IsInCluster()
                    ? KubernetesClientConfiguration.InClusterConfig()
                    : KubernetesClientConfiguration.BuildConfigFromConfigFile()));
        }

        public bool Enabled => _options.Enabled;
        public int MaxReplicas => _options.MaxReplicas;

        public async Task<WorkerScale> GetScale(string deployment, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return new WorkerScale(deployment, 0, 0, false);

            try
            {
                V1Deployment found = await _client.Value.AppsV1.ReadNamespacedDeploymentAsync(deployment, _options.Namespace, cancellationToken: cancellationToken);
                return new WorkerScale(deployment, found.Spec?.Replicas ?? 0, found.Status?.ReadyReplicas ?? 0, true);
            }
            catch (HttpOperationException)
            {
                return new WorkerScale(deployment, 0, 0, false);
            }
        }

        public async Task<WorkerScale> SetScale(string deployment, int replicas, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                throw new InvalidOperationException("Worker scaling is disabled.");

            if (replicas < 0 || replicas > _options.MaxReplicas)
                throw new ArgumentOutOfRangeException(nameof(replicas), replicas, $"Replicas must be between 0 and {_options.MaxReplicas}.");

            if (!_options.Deployments.ContainsValue(deployment))
                throw new ArgumentException($"'{deployment}' is not a known worker deployment.", nameof(deployment));

            V1Patch patch = new($"{{\"spec\":{{\"replicas\":{replicas}}}}}", V1Patch.PatchType.MergePatch);

            try
            {
                await _client.Value.AppsV1.PatchNamespacedDeploymentScaleAsync(patch, deployment, _options.Namespace, cancellationToken: cancellationToken);

                V1Deployment scaled = await _client.Value.AppsV1.ReadNamespacedDeploymentAsync(deployment, _options.Namespace, cancellationToken: cancellationToken);
                return new WorkerScale(deployment, scaled.Spec?.Replicas ?? 0, scaled.Status?.ReadyReplicas ?? 0, true);
            }
            catch (HttpOperationException ex)
            {
                throw new KeyNotFoundException($"Deployment '{deployment}' not found in namespace '{_options.Namespace}'. ({ex.Response.StatusCode})");
            }
        }
    }
}
