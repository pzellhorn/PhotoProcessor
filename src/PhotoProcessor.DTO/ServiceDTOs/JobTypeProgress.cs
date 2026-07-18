using PhotoProcessor.DTO.enums;

namespace PhotoProcessor.DTO.ServiceDTOs
{
    public class JobTypeProgress
    {
        public JobTypes JobType { get; set; }
        public MediaItemType AppliesTo { get; set; }
        public string Queue { get; set; } = string.Empty;

        public uint QueueDepth { get; set; }
        public uint Consumers { get; set; }
        public bool QueueExists { get; set; }

        public int EligibleMedia { get; set; }
        public int Done { get; set; }
        public int Queued { get; set; }
        public int Running { get; set; }
        public int Failed { get; set; }
        public int NeverRun { get; set; }

        public string Deployment { get; set; } = string.Empty;
        public bool ScalingEnabled { get; set; }
        public bool DeploymentFound { get; set; }
        public int Replicas { get; set; }
        public int ReadyReplicas { get; set; }
        public int MaxReplicas { get; set; }
    }
}
