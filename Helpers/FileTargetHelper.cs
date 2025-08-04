using Spectre.Console;
using ZeroTrace.Enums;
using ZeroTrace.Model;

namespace ZeroTrace.Helpers;

public static class FileTargetHelper
{
    public static List<string> GetFiles(List<TargetItem> targets)
    {
        List<string> filePaths = [];

        foreach (var target in targets)
        {
            if (target.Type != TargetType.File)
            {
                try
                {
                    filePaths.AddRange(GetSafeFiles(target.Path));
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error accessing {target.Path}: {ex.Message}[/]");
                }
            }
            else
            {
                filePaths.Add(target.Path);
            }
        }

        return filePaths;
    }

    private static List<string> GetSafeFiles(string path)
    {
        var files = new List<string>();
        try
        {
            files.AddRange(Directory.GetFiles(path));
            foreach (var dir in Directory.GetDirectories(path))
            {
                try
                {
                    files.AddRange(GetSafeFiles(dir));
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
        }

        return files;
    }

    public static void CleanUpDirectories(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            return;
        }
        var directories = GetDirectories(rootPath);
        foreach (var directory in directories)
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch (Exception)
            {

            }
        }
    }
    private static string[] GetDirectories(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            return [];
        }

        try
        {
            return Directory.GetDirectories(rootPath);
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
        catch (PathTooLongException)
        {
            return [];
        }

    }
}
