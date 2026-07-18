namespace PhotoProcessor.Logic.Encoding
{
    public interface ITextEncoder
    {
        Task<float[]> EncodeText(string text, CancellationToken cancellationToken = default);
    }
}
