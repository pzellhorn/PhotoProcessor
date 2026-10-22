using SkiaSharp;

namespace PhotoProcessor.Logic.Imaging
{
    /// <summary>
    /// Decodes images with the correct orientation applied, taken from EXIF
    /// </summary>
    public static class UprightBitmap
    {
        public static SKBitmap? Decode(Stream stream)
        {
            using SKCodec? codec = SKCodec.Create(stream);
            if (codec is null)
                return null;

            SKBitmap? decoded = SKBitmap.Decode(codec);
            if (decoded is null || codec.EncodedOrigin == SKEncodedOrigin.TopLeft)
                return decoded;

            using (decoded)
                return Orient(decoded, codec.EncodedOrigin);
        }

        private static SKBitmap Orient(SKBitmap decoded, SKEncodedOrigin origin)
        {
            bool swapsAxes = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
            int width = swapsAxes ? decoded.Height : decoded.Width;
            int height = swapsAxes ? decoded.Width : decoded.Height;

            SKBitmap upright = new(decoded.Info.WithSize(width, height));
            using SKCanvas canvas = new(upright);
            canvas.SetMatrix(OriginToMatrix(origin, width, height));
            canvas.DrawBitmap(decoded, 0, 0, SKSamplingOptions.Default);
            return upright;
        }
         
        private static SKMatrix OriginToMatrix(SKEncodedOrigin origin, int width, int height) => origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, width, 0, 1, 0, 0, 0, 1),
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, width, 0, -1, height, 0, 0, 1),
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, height, 0, 0, 1),
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, width, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, width, -1, 0, height, 0, 0, 1),
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, height, 0, 0, 1),
            _ => SKMatrix.Identity,
        };
    }
}
