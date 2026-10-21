using System.Buffers.Binary;

namespace PhotoProcessor.Logic.Imaging
{
    internal static class JpegOrientation
    { 
        private const int ExifPayloadLength = 6 + 8 + 2 + 12 + 4;

        /// <summary>
        /// Inserts photo orientation metadata for jpegs that don't have it already defined
        /// </summary>
        public static byte[] WithOrientation(byte[] jpeg, ushort orientation)
        {
            byte[] segment = new byte[2 + 2 + ExifPayloadLength];
            Span<byte> span = segment;

            span[0] = 0xFF;
            span[1] = 0xE1;
            BinaryPrimitives.WriteUInt16BigEndian(span[2..], 2 + ExifPayloadLength);
            "Exif\0\0"u8.CopyTo(span[4..]);

            Span<byte> tiff = span[10..];
            "II"u8.CopyTo(tiff);
            BinaryPrimitives.WriteUInt16LittleEndian(tiff[2..], 42);
            BinaryPrimitives.WriteUInt32LittleEndian(tiff[4..], 8);
            BinaryPrimitives.WriteUInt16LittleEndian(tiff[8..], 1);
            BinaryPrimitives.WriteUInt16LittleEndian(tiff[10..], TiffPreviewReader.OrientationTag);
            BinaryPrimitives.WriteUInt16LittleEndian(tiff[12..], 3);
            BinaryPrimitives.WriteUInt32LittleEndian(tiff[14..], 1);
            BinaryPrimitives.WriteUInt16LittleEndian(tiff[18..], orientation);
            BinaryPrimitives.WriteUInt32LittleEndian(tiff[22..], 0);

            byte[] result = new byte[jpeg.Length + segment.Length];
            jpeg.AsSpan(0, 2).CopyTo(result);
            segment.CopyTo(result, 2);
            jpeg.AsSpan(2).CopyTo(result.AsSpan(2 + segment.Length));
            return result;
        }
    }
}
