using Spectre.Console;
using System.Security.Cryptography;

namespace ZeroTrace.Services;

internal class PartitionDeleteService : DeleteBase
{
    public static event Action? ProgressChanged;
    public static event Action? Completed;
    public static void DeletePartition(string partitionPath, int passes = 7)
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

        var path = Path.Combine(partitionPath, "zerotrace.tmp");
        using var stream = new FileStream(path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                FileOptions.SequentialScan);
        byte[] buffer = new byte[bufferSize];
        DriveInfo drive = new(partitionPath);
        long freeSpace = drive.AvailableFreeSpace;
        while (freeSpace > 0)
        {
            try
            {
                int writeSize = (int)Math.Min(buffer.Length, freeSpace);
                RandomNumberGenerator.Fill(buffer.AsSpan(0, writeSize));
                stream.Write(buffer, 0, writeSize);
                freeSpace -= writeSize;
                ProgressChanged?.Invoke();
            }
            catch (IOException)
            {
                break;
            }
        }
        Completed?.Invoke();
        stream.Flush();
        stream.Close();

        FileDeleteService.Delete([path], passes);
    }
}