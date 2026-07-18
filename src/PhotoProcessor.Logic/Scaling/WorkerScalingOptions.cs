using PhotoProcessor.DTO.enums;

namespace PhotoProcessor.Logic.Scaling
{
    public class WorkerScalingOptions
    {
        public bool Enabled { get; set; }
        public string Namespace { get; set; } = "default";
        public int MaxReplicas { get; set; } = 5;

        public Dictionary<JobTypes, string> Deployments { get; set; } = new();
    }
}
