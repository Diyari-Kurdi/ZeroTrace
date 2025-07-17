using Spectre.Console;
using System.Security.Cryptography;

namespace ZeroTrace.Services;

internal class FileDeleteService : DeleteBase
{
    public static event Action<string>? FileOverwrited;
    public static event Action<int>? PassCompleted;

    public static void Delete(string[] files, int passes = 7)
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
}
