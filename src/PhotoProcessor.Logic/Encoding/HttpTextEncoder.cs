using System.Net.Http.Json;

namespace PhotoProcessor.Logic.Encoding
{
    public class HttpTextEncoder(HttpClient httpClient) : ITextEncoder
    {
        private sealed class EncodeResponse
        {
            public float[] Embedding { get; set; } = [];
        }

        public async Task<float[]> EncodeText(string text, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/encode", new { text }, cancellationToken);
            response.EnsureSuccessStatusCode();

            EncodeResponse encoded = await response.Content.ReadFromJsonAsync<EncodeResponse>(cancellationToken)
                ?? throw new InvalidOperationException("Text encoder returned an empty response.");

            return encoded.Embedding;
        }
    }
}
