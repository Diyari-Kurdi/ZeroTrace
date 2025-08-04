using System.Security.Cryptography;

namespace ZeroTrace.Helpers;

public static class FileWipeUtilities
{
    public static int GetBufferSize(long fileLength = 0)
    {
        var defaultBufferSize = 64 * 1024 * 1024;

        if (fileLength == 0)
            return defaultBufferSize;

        if (fileLength < 1 * 1024 * 1024)
            return 64 * 1024;

        if (fileLength < 10 * 1024 * 1024)
            return 512 * 1024;

        return defaultBufferSize;
    }


    public static int GetRandomInt(RandomNumberGenerator rng, int min, int max)
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
        while (result >= int.MaxValue - int.MaxValue % range);

        return min + result % range;
    }
    public static long GetRandomLong(RandomNumberGenerator rng, long min, long max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);

        byte[] bytes = new byte[8];
        long range = max - min;
        long result;

        do
        {
            rng.GetBytes(bytes);
            result = BitConverter.ToInt64(bytes, 0) & long.MaxValue;
        } while (result >= long.MaxValue - long.MaxValue % range);

        return min + result % range;
    }
}
