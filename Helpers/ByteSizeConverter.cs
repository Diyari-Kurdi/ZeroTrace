namespace ZeroTrace.Helpers;

public static class ByteSizeConverter
{
    private static readonly string[] _units = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

    public static string ToHumanReadable(long bytes)
    {
        if (bytes < 0)
            return "Invalid";

        double size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < _units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {_units[unitIndex]}";
    }
}
