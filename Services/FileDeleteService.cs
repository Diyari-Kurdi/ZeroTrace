using Spectre.Console;
using System.Security.Cryptography;

namespace ZeroTrace.Services;

internal class FileDeleteService : DeleteBase
{
    public static event Action<string>? FileOverwrited;
    public static event Action<int>? PassCompleted;

    public static void Delete(IReadOnlyList<string> filePaths, int passes = 7)
    {
        Parallel.ForEach(filePaths, filePath =>
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return;
                }

                DeleteFile(filePath, passes);
            }
            catch (Exception ex)
            {
                lock (AnsiConsole.Console)
                {
                    AnsiConsole.MarkupLine($"[red]Error deleting {filePath}: {ex.Message}[/]");
                }
            }
        });
    }

    private static void DeleteFile(string filePath, int passes)
    {
        using var rng = RandomNumberGenerator.Create();


        int? lastMethod = null;

        for (int i = 0; i < passes; i++)
        {
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, GetBufferSize(new FileInfo(filePath).Length), FileOptions.SequentialScan);

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
                    Reverse(stream);
                    break;
                case 1:
                    Shake(stream, rng);
                    break;
                case 2:
                    ZeroFill(stream);
                    break;
                case 3:
                    Random(stream, rng);
                    break;
                default:
                    Random(stream, rng);
                    break;
            }

            lastMethod = method;
            PassCompleted?.Invoke(i + 1);
            stream.Close();

        }

        try
        {
            File.Delete(filePath);
        }
        catch (Exception)
        {
            
        }
        finally
        {
            FileOverwrited?.Invoke(filePath);
        }
    }
}
