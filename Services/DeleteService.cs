using Spectre.Console;
using System.Security.Cryptography;

namespace ZeroTrace.Services;

internal static class DeleteService
{
    public static event Action<string>? FileOverwrited;
    public static event Action<int>? PassCompleted;
    public static void Delete(string[] files, int passes = 7, int bufferSize = 4096)
    {
        foreach (string filePath in files)
        {
            using var rng = RandomNumberGenerator.Create();
            if (!File.Exists(filePath))
            {
                AnsiConsole.Write(new TextPath(filePath)
                .LeafColor(Color.Yellow)
                .SeparatorColor(Color.Green)
                .RootColor(Color.Red)
                .StemColor(Color.Blue));
                AnsiConsole.WriteLine($" not found!");
                continue;
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, bufferSize, useAsync: true);

            int? lastMethod = null;

            for (int i = 0; i < passes; i++)
            {
                int method;
                do
                {
                    method = GetRandomInt(rng, 0, 5);
                }
                while ((lastMethod == 0 && method == 0) ||
                         (lastMethod == 2 && method == 2));

                switch (method)
                {
                    case 0:
                        Reverse(stream, bufferSize);
                        break;
                    case 1:
                        Shake(stream, rng);
                        break;
                    case 2:
                        ZeroFill(stream, bufferSize);
                        break;
                    case 3:
                        Random(stream, rng, bufferSize);
                        break;
                    default:
                        Random(stream, rng, bufferSize);
                        break;
                }

                lastMethod = method;
                PassCompleted?.Invoke(i + 1);
            }
            stream.Close();

            try
            {
                File.Delete(filePath);
                AnsiConsole.Write($"File ");
                AnsiConsole.Write(new TextPath(filePath)
                    .LeafColor(Color.Yellow)
                    .SeparatorColor(Color.Green)
                    .RootColor(Color.Red)
                    .StemColor(Color.Blue));
                AnsiConsole.WriteLine($" deleted.");
            }
            catch (Exception ex)
            {
                AnsiConsole.Write($"File ");
                AnsiConsole.Write(new TextPath(filePath)
                    .LeafColor(Color.Yellow)
                    .SeparatorColor(Color.Green)
                    .RootColor(Color.Red)
                    .StemColor(Color.Blue));
                AnsiConsole.WriteLine($" overwrited but could not be deleted.");
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
            finally
            {
                FileOverwrited?.Invoke(filePath);
            }
        }
    }

    public static void DeletePartition(string partitionPath, int passes = 7, int bufferSize = 4096 * 1024)
    {
        if (!Directory.Exists(partitionPath))
        {
            AnsiConsole.Write(new TextPath(partitionPath)
                .LeafColor(Color.Yellow)
                .SeparatorColor(Color.Green)
                .RootColor(Color.Red)
                .StemColor(Color.Blue));
            AnsiConsole.WriteLine($" not found!");
            return;
        }
        var files = Directory.GetFiles(partitionPath, "*", SearchOption.AllDirectories);
        Delete(files, passes, bufferSize);

        var path = Path.Combine(partitionPath, "zerotrace.tmp");
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        byte[] buffer = new byte[bufferSize];
        RandomNumberGenerator.Fill(buffer);

        while (true)
        {
            try
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
                break;
            }
        }
        stream.Flush();
        stream.Close();
        Delete([path], passes, bufferSize);

    }

    private static void Reverse(FileStream stream, int bufferSize)
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

    private static void Shake(FileStream stream, RandomNumberGenerator rng, int intensity = 500)
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

    private static void ZeroFill(FileStream stream, int bufferSize)
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

    private static void Random(FileStream stream, RandomNumberGenerator rng, int bufferSize)
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

    private static int GetRandomInt(RandomNumberGenerator rng, int min, int max)
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
    private static long GetRandomLong(RandomNumberGenerator rng, long min, long max)
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
