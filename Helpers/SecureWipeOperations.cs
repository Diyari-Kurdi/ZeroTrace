using System.Security.Cryptography;

namespace ZeroTrace.Helpers;

public static class SecureWipeOperations
{
    public static void Reverse(FileStream stream)
    {
        int left = 0;
        long right = stream.Length - 1;

        int bufferSize = FileWipeUtilities.GetBufferSize(stream.Length);
        byte[] leftBuffer = new byte[bufferSize];
        byte[] rightBuffer = new byte[bufferSize];

        while (left < right)
        {
            int blockSize = (int)Math.Min(FileWipeUtilities.GetBufferSize(stream.Length), (right - left + 1) / 2);

            stream.Position = left;
            stream.ReadExactly(leftBuffer, 0, blockSize);

            stream.Position = right - blockSize + 1;
            stream.ReadExactly(rightBuffer, 0, blockSize);

            stream.Position = left;
            for (int i = blockSize - 1; i >= 0; i--)
            {
                stream.WriteByte(rightBuffer[i]);
            }

            stream.Position = right - blockSize + 1;
            for (int i = blockSize - 1; i >= 0; i--)
            {
                stream.WriteByte(leftBuffer[i]);
            }

            left += blockSize;
            right -= blockSize;
        }

        stream.Flush();
    }

    public static void Shake(FileStream stream, RandomNumberGenerator rng, int intensity = 500)
    {
        long length = stream.Length;
        if (length < 2) return;

        for (int i = 0; i < intensity; i++)
        {
            long pos1 = FileWipeUtilities.GetRandomLong(rng, 0, length);
            long pos2 = FileWipeUtilities.GetRandomLong(rng, 0, length);

            stream.Position = pos1;
            int b1 = stream.ReadByte();
            stream.Position = pos2;
            int b2 = stream.ReadByte();

            if (b1 == -1 || b2 == -1) continue;

            stream.Position = pos1;
            stream.WriteByte((byte)b2);
            stream.Position = pos2;
            stream.WriteByte((byte)b1);
        }

        stream.Flush();
    }

    public static void ZeroFill(FileStream stream)
    {
        var bufferSize = FileWipeUtilities.GetBufferSize(stream.Length);
        byte[] buffer = new byte[bufferSize];
        long length = stream.Length;
        stream.Position = 0;

        while (length > 0)
        {
            int toWrite = (int)Math.Min(bufferSize, length);
            stream.Write(buffer, 0, toWrite);
            length -= toWrite;
        }

        stream.Flush();
    }

    public static void Random(FileStream stream, RandomNumberGenerator rng)
    {
        var bufferSize = FileWipeUtilities.GetBufferSize(stream.Length);
        byte[] buffer = new byte[bufferSize];
        long length = stream.Length;
        stream.Position = 0;

        while (length > 0)
        {
            int toWrite = (int)Math.Min(bufferSize, length);
            rng.GetBytes(buffer, 0, toWrite);
            stream.Write(buffer, 0, toWrite);
            length -= toWrite;
        }

        stream.Flush();
    }
}
