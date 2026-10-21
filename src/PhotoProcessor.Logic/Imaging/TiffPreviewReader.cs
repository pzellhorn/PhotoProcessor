using System.Buffers.Binary;

namespace PhotoProcessor.Logic.Imaging
{
    /// <summary>
    ///  TIFF-structured files are files like NEF (ie: the raw photo data from my Nikon camera). This grabs the JPEG previews inside those formats.
    /// </summary>
    internal static class TiffPreviewReader
    {
        public const ushort OrientationTag = 0x0112;
        public const ushort DefaultOrientation = 1;

        private const ushort SubIfdsTag = 0x014A;
        private const ushort JpegOffsetTag = 0x0201;
        private const ushort JpegLengthTag = 0x0202;

        private const ushort ShortType = 3;
        private const int EntrySize = 12;
        private const int MaxIfds = 64;

        public static bool IsJpeg(byte[] data, long offset, long length) =>
            length >= 3 && offset >= 0 && offset + length <= data.Length
            && data[offset] == 0xFF && data[offset + 1] == 0xD8 && data[offset + 2] == 0xFF;

        /// <summary>
        /// Takes the largest jpeg found inside the Image File Directory on a TIFF file
        /// </summary>
        public static bool TryReadLargestJpeg(byte[] data, out byte[] preview, out ushort orientation)
        {
            preview = [];
            orientation = DefaultOrientation;
            if (data.Length < 8)
                return false;

            bool littleEndian;
            if (data[0] == 'I' && data[1] == 'I')
                littleEndian = true;
            else if (data[0] == 'M' && data[1] == 'M')
                littleEndian = false;
            else
                return false;

            if (ReadUInt16(data, 2, littleEndian) != 42)
                return false;

            long ifd0 = ReadUInt32(data, 4, littleEndian);
            Queue<long> pending = new();
            pending.Enqueue(ifd0);
            HashSet<long> visited = new();

            long bestOffset = 0;
            long bestLength = 0;

            while (pending.Count > 0 && visited.Count < MaxIfds)
            {
                long ifd = pending.Dequeue();
                if (ifd <= 0 || ifd + 2 > data.Length || !visited.Add(ifd))
                    continue;

                int entryCount = ReadUInt16(data, ifd, littleEndian);
                long jpegOffset = 0;
                long jpegLength = 0;

                for (int i = 0; i < entryCount; i++)
                {
                    long entry = ifd + 2 + (long)i * EntrySize;
                    if (entry + EntrySize > data.Length)
                        break;

                    ushort tag = ReadUInt16(data, entry, littleEndian);
                    switch (tag)
                    {
                        case OrientationTag when ifd == ifd0:
                            orientation = (ushort)ReadValue(data, entry, littleEndian);
                            break;
                        case JpegOffsetTag:
                            jpegOffset = ReadValue(data, entry, littleEndian);
                            break;
                        case JpegLengthTag:
                            jpegLength = ReadValue(data, entry, littleEndian);
                            break;
                        case SubIfdsTag:
                            foreach (long subIfd in ReadOffsets(data, entry, littleEndian))
                                pending.Enqueue(subIfd);
                            break;
                    }
                }

                if (jpegLength > bestLength && IsJpeg(data, jpegOffset, jpegLength))
                {
                    bestOffset = jpegOffset;
                    bestLength = jpegLength;
                }

                long nextIfdPointer = ifd + 2 + (long)entryCount * EntrySize;
                if (nextIfdPointer + 4 <= data.Length)
                    pending.Enqueue(ReadUInt32(data, nextIfdPointer, littleEndian));
            }

            if (bestLength == 0)
                return false;

            preview = data.AsSpan((int)bestOffset, (int)bestLength).ToArray();
            return true;
        }

        private static long ReadValue(byte[] data, long entry, bool littleEndian)
        {
            ushort type = ReadUInt16(data, entry + 2, littleEndian);
            return type == ShortType ? ReadUInt16(data, entry + 8, littleEndian) : ReadUInt32(data, entry + 8, littleEndian);
        }

        private static List<long> ReadOffsets(byte[] data, long entry, bool littleEndian)
        {
            long count = ReadUInt32(data, entry + 4, littleEndian);
            long valuesAt = count <= 1 ? entry + 8 : ReadUInt32(data, entry + 8, littleEndian);

            List<long> offsets = new();
            for (long i = 0; i < Math.Min(count, MaxIfds); i++)
            {
                long position = valuesAt + i * 4;
                if (position + 4 > data.Length)
                    break;
                offsets.Add(ReadUInt32(data, position, littleEndian));
            }
            return offsets;
        }

        private static ushort ReadUInt16(byte[] data, long offset, bool littleEndian)
        {
            ReadOnlySpan<byte> bytes = data.AsSpan((int)offset, 2);
            return littleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(bytes) : BinaryPrimitives.ReadUInt16BigEndian(bytes);
        }

        private static long ReadUInt32(byte[] data, long offset, bool littleEndian)
        {
            ReadOnlySpan<byte> bytes = data.AsSpan((int)offset, 4);
            return littleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(bytes) : BinaryPrimitives.ReadUInt32BigEndian(bytes);
        }
    }
}
