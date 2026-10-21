namespace PhotoProcessor.Logic.Imaging
{
    public interface IFileConverter
    {
        //Converts one file format to the target format
        Task<Stream> Convert(Stream source, FileFormat target, CancellationToken cancellationToken = default);
    }
}
