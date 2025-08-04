using Spectre.Console;
using ZeroTrace.Model;

namespace ZeroTrace.Helpers;

internal static class PartitionDeleteService
{
    public static event Action? ProgressChanged;
    public static event Action? Completed;
    public static void FillPartitionWithTempData(string partitionPath)
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
        const int maxFileSize = 100 * 1024 * 1024;

        var bufferSize = FileWipeUtilities.GetBufferSize();
        byte[] buffer = new byte[bufferSize];
        DriveInfo drive = new(partitionPath);
        long freeSpace = drive.TotalFreeSpace;
        int fileIndex = 0;
        var tempFiles = new List<TargetItem>();
        while (freeSpace > 0)
        {
            var fileName = $"{Guid.NewGuid()}.tmp";
            var filePath = Path.Combine(partitionPath, fileName);

            try
            {
                long fileRemaining = Math.Min(maxFileSize, freeSpace);

                using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan);

                while (fileRemaining > 0)
                {
                    int writeSize = (int)Math.Min(bufferSize, fileRemaining);
                    stream.Write(buffer, 0, writeSize);

                    fileRemaining -= writeSize;
                    freeSpace -= writeSize;

                    ProgressChanged?.Invoke();
                }

                stream.Flush();
                tempFiles.Add(new TargetItem(filePath, Enums.TargetType.File, ByteSizeConverter.ToHumanReadable(stream.Length)));
                fileIndex++;
            }
            catch (IOException)
            {
                break;
            }
        }


        Completed?.Invoke();
    }
}