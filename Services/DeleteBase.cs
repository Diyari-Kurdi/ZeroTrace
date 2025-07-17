using System.Security.Cryptography;

namespace ZeroTrace.Services;

public abstract class DeleteBase
{
    protected const int bufferSize = 64 * 1024 * 1024;
    protected static void Reverse(FileStream stream)
    {
        int left = 0;
        long right = stream.Length - 1;

        byte[] leftBuffer = new byte[stream.Length];
        byte[] rightBuffer = new byte[stream.Length];

        while (left < right)
        {
            int blockSize = (int)Math.Min(bufferSize, (right - left + 1) / 2);

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

    protected static void Shake(FileStream stream, RandomNumberGenerator rng, int intensity = 500)
    {
        long length = stream.Length;
        if (length < 2) return;

        for (int i = 0; i < intensity; i++)
        {
            long pos1 = GetRandomLong(rng, 0, length);
            long pos2 = GetRandomLong(rng, 0, length);

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

    protected static void ZeroFill(FileStream stream)
    {
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

    protected static void Random(FileStream stream, RandomNumberGenerator rng)
    {
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

    protected static int GetRandomInt(RandomNumberGenerator rng, int min, int max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);

        byte[] bytes = new byte[4];
        int range = max - min;
        int result;

        do
        {
            rng.GetBytes(bytes);
            result = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
        }
        while (result >= (int.MaxValue - (int.MaxValue % range)));

        return min + (result % range);
    }
    protected static long GetRandomLong(RandomNumberGenerator rng, long min, long max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);

        byte[] bytes = new byte[8];
        long range = max - min;
        long result;

        do
        {
            rng.GetBytes(bytes);
            result = BitConverter.ToInt64(bytes, 0) & long.MaxValue;
        } while (result >= (long.MaxValue - (long.MaxValue % range)));

        return min + (result % range);
    }
}
