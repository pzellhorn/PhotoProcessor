using PhotoProcessor.Logic.Encoding;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IFlowerGate
    {
        /// <summary>Probability that a photo shows flowers </summary>
        Task<double> Score(float[] imageEmbedding, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 
    /// uses clip embedding to decide if the image is a flower or not, as BioCLIP just does best effort with whatever is on the sent video frame
    /// </summary>
    public class FlowerGate(ITextEncoder textEncoder, FlowerLabelCache labelCache) : IFlowerGate
    { 
        private const double LogitScale = 100.0;

        private static readonly string[] FlowerLabels = ["flower", "flowers", "a flowering plant", "wildflowers", "a blossom"];

        private static readonly string[] OtherLabels =
        [
            "person", "people", "dog", "cat", "animal", "bird", "insect", "car", "building", "street", "city", "road", "snow",
            "forest", "trees", "grass", "leaves", "a plant without flowers", "a fern", "a shrub", "landscape", "mountain", "sky",
            "water", "a room", "a sign", "text", "food", "a toy", "a path", "rocks", "darkness",
        ];

        private static readonly string[] Templates = ["a photo of {0}.", "a close-up photo of {0}.", "a blurry photo of {0}."];

        public async Task<double> Score(float[] imageEmbedding, CancellationToken cancellationToken = default)
        {
            float[][] labels = await labelCache.GetOrCreate(() => EmbedLabels(cancellationToken));

            double[] logits = new double[labels.Length];
            double maxLogit = double.MinValue;
            for (int i = 0; i < labels.Length; i++)
            {
                logits[i] = LogitScale * Dot(labels[i], imageEmbedding);
                maxLogit = Math.Max(maxLogit, logits[i]);
            }

            double total = 0;
            double flower = 0;
            for (int i = 0; i < logits.Length; i++)
            {
                double weight = Math.Exp(logits[i] - maxLogit);
                total += weight;
                if (i < FlowerLabels.Length)
                    flower += weight;
            }
            return flower / total;
        }

        private async Task<float[][]> EmbedLabels(CancellationToken cancellationToken)
        {
            List<float[]> labels = new();
            foreach (string label in FlowerLabels.Concat(OtherLabels))
            {
                float[]? sum = null;
                foreach (string template in Templates)
                {
                    float[] prompt = Normalize(await textEncoder.EncodeText(string.Format(template, label), cancellationToken));
                    sum ??= new float[prompt.Length];
                    for (int i = 0; i < prompt.Length; i++)
                        sum[i] += prompt[i];
                }
                labels.Add(Normalize(sum!));
            }
            return labels.ToArray();
        }

        private static double Dot(float[] a, float[] b)
        {
            if (a.Length != b.Length)
                throw new InvalidOperationException($"Label embedding has {a.Length} dimensions but the image embedding has {b.Length}.");

            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += a[i] * b[i];
            return sum;
        }

        private static float[] Normalize(float[] vector)
        {
            double length = Math.Sqrt(Dot(vector, vector));
            float[] normalized = new float[vector.Length];
            for (int i = 0; i < vector.Length; i++)
                normalized[i] = (float)(vector[i] / length);
            return normalized;
        }
    }
     
    public class FlowerLabelCache
    {
        private readonly Lock syncRoot = new();
        private Task<float[][]>? labels;

        public Task<float[][]> GetOrCreate(Func<Task<float[][]>> factory)
        {
            lock (syncRoot)
            {
                if (labels is null || labels.IsFaulted || labels.IsCanceled)
                    labels = factory();
                return labels;
            }
        }
    }
}
