using SkiaSharp;

namespace PhotoProcessor.Logic.Imaging
{ 
    public class ImageFileConverter : IFileConverter
    {
        private const int JpegQuality = 100; 
        private const int MinPreviewLongEdge = 1024;    //Min pixel count across the longest edge of the image

        public async Task<Stream> Convert(Stream source, FileFormat target, CancellationToken cancellationToken = default)
        {
            if (target != FileFormat.Jpeg)
                throw new ArgumentOutOfRangeException(nameof(target), target, $"{nameof(ImageFileConverter)} cannot produce {target}.");

            using MemoryStream buffer = new();
            await source.CopyToAsync(buffer, cancellationToken);
            byte[] data = buffer.ToArray();

            if (TiffPreviewReader.IsJpeg(data, 0, data.Length))
                return new MemoryStream(data, writable: false);

            if (TiffPreviewReader.TryReadLargestJpeg(data, out byte[] preview, out ushort orientation))
                return FromPreview(preview, orientation);

            return Reencode(data);
        }

        private static MemoryStream FromPreview(byte[] preview, ushort orientation)
        {
            using SKCodec? codec = SKCodec.Create(new SKMemoryStream(preview));
            if (codec is null || Math.Max(codec.Info.Width, codec.Info.Height) < MinPreviewLongEdge)
                throw new NotSupportedException("Raw image has no usable embedded preview.");

            if (orientation != TiffPreviewReader.DefaultOrientation && codec.EncodedOrigin == SKEncodedOrigin.TopLeft)
                preview = JpegOrientation.WithOrientation(preview, orientation);

            return new MemoryStream(preview, writable: false);
        }

        private static MemoryStream Reencode(byte[] data)
        {
            using SKBitmap bitmap = UprightBitmap.Decode(new MemoryStream(data, writable: false)) ?? throw new InvalidOperationException("Could not decode image.");
            using SKImage image = SKImage.FromBitmap(bitmap);
            using SKData encoded = image.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);

            MemoryStream output = new();
            encoded.SaveTo(output);
            output.Position = 0;
            return output;
        }
    }
}
