using Spectre.Console;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ZeroTrace.Enums;
using ZeroTrace.Helpers;
using ZeroTrace.Model;

namespace ZeroTrace.Views;

public static partial class FileSelection
{
    public static void Show()
    {
        List<TargetItem> targets = [];
        while (true)
        {
            var newTargets = AskForTarget().Where(nt => !targets.Any(t => t.Path == nt.Path));
            AnsiConsole.Clear();
            About.Show();
            targets.AddRange(newTargets);

            if (targets.Count == 0)
            {
                return;
            }

            ShowSelectedTargets(targets);

            var result = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .AddChoices("Add more", "Clear All", "Continue"));


            if (result.StartsWith("Clear"))
            {
                targets.Clear();
                AnsiConsole.Clear();
                About.Show();
            }
            else if (result.StartsWith("Continue"))
            {
                var passes = AnsiConsole.Prompt(
                     new TextPrompt<int>("[green]Enter number of overwrite passes (More = Slower & Safer) [7]:[/]")
                         .DefaultValue(7)
                         .Validate(p => p > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Passes must be greater than 0[/]")));
                var confirmation = AnsiConsole.Prompt(
                    new TextPrompt<bool>("[bold red]Are you sure you want to securely delete the selected targets?[/]")
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(false)
                        .WithConverter(choice => choice ? "y" : "n"));
                AnsiConsole.WriteLine();
                if (confirmation)
                {
                    AnsiConsole.Progress()
                        .Start(ctx =>
                        {
                            List<string> files = FileTargetHelper.GetFiles(targets);

                            var task1 = ctx.AddTask($"[green]Target files[/]", true, files.Count);
                            var task2 = ctx.AddTask($"[green]{passes} passes to complete[/]", true, passes);

                            FileDeleteService.FileOverwrited += file =>
                            {
                                task1.Increment(1);
                            };
                            FileDeleteService.PassCompleted += (pass) =>
                            {
                                task2.Value(pass);
                            };

                            FileDeleteService.Delete(files.AsReadOnly(), passes);
                        });

                    AnsiConsole.Prompt(
                        new TextPrompt<string>("[bold green]Deletation completed![/] Press Enter... ")
                        .DefaultValue(string.Empty)
                        .HideDefaultValue());

                    targets.Clear();
                    AnsiConsole.Clear();
                    About.Show();
                }
                else
                {
                    AnsiConsole.Clear();
                    About.Show();
                    ShowSelectedTargets(targets);
                }

            }
        }

    }

    private static void ShowSelectedTargets(List<TargetItem> targets)
    {
        var table = new Table()
                .Border(TableBorder.Rounded)
                .Title("[green]Selected Targets[/]")
                .AddColumn("Type")
                .AddColumn("Path")
                .AddColumn("Size");

        foreach (var item in targets)
        {
            var type = new Markup(GetTypeName(item.Path));
            var path = new TextPath(item.Path)
                .LeafColor(Color.Yellow)
                .SeparatorColor(Color.Green)
                .RootColor(Color.Red)
                .StemColor(Color.Blue);
            var size = new Text(item.Size);

            table.AddRow(type, path, size);
        }

        AnsiConsole.Write(table);
    }
    private static List<TargetItem> AskForTarget()
    {
        var systemRoot = Path.GetPathRoot(Environment.SystemDirectory);
        var allDrives = DriveInfo.GetDrives();
        IEnumerable<DriveInfo> drives;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            drives = allDrives.Where(d =>
                (d.DriveType == DriveType.Removable || d.DriveType == DriveType.Fixed)
                && d.IsReady
                && !d.Name.Equals(systemRoot, StringComparison.OrdinalIgnoreCase));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            drives = allDrives.Where(d =>
                (d.DriveType == DriveType.Removable || d.DriveType == DriveType.Fixed ||
                 d.DriveType == DriveType.Unknown)
                && d.IsReady
                && d.Name.StartsWith("/Volumes/"));
        }
        else
        {
            drives = allDrives.Where(d =>
                (d.DriveType == DriveType.Removable || d.DriveType == DriveType.Fixed ||
                 d.DriveType == DriveType.Unknown)
                && d.IsReady
                && d.Name.StartsWith("/media/") || d.Name.StartsWith("/mnt/"));
        }
        drives = [.. drives];

        var choices = drives
            .Select(d =>
            {
                string label = string.IsNullOrWhiteSpace(d.VolumeLabel) ? "No Label" : d.VolumeLabel;
                string size = ByteSizeConverter.ToHumanReadable(d.TotalSize);
                return new UserChoise($":computer_disk: {d.Name} ({label} - {size})", new TargetItem(d.Name, TargetType.Directory, size));
            })
            .ToList();

        var names = drives.Select(drive => drive.Name).ToList();
        choices.Add(new(":open_file_folder: Manually enter", null));
        choices.Add(new(":back_arrow: Go Back", null));


        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<UserChoise>()
                .Title("[bold red]Choose target to securely delete:[/]")
                .AddChoices(choices).UseConverter(p => p.DisplayText));

        if (selected.DisplayText.StartsWith(":open_file_folder:"))
        {
            var input = AnsiConsole.Ask<string>(
            "Drag & drop or manually enter file/folder paths (separated by [green];[/], space or newline):");
            var model = ParsePathsToModel(input);
            return model;
        }
        else if (selected.DisplayText.StartsWith(":back_arrow:"))
        {
            return [];
        }

        return [selected.Item!];
    }

    private static List<TargetItem> ParsePathsToModel(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        var matches = PathRegex().Matches(input);
        var paths = matches
            .Select(m =>
                m.Groups[1].Success ? m.Groups[1].Value :
                m.Groups[2].Success ? m.Groups[2].Value :
                m.Groups[3].Value
            )
            .Distinct()
            .Where(Path.Exists)
            .ToList();

        return [.. paths.Select(BuildTargetItem)];
    }

    private static TargetItem BuildTargetItem(string path)
    {
        TargetType type;
        long size = 0;

        try
        {
            if (File.Exists(path))
            {
                type = TargetType.File;
                size = new FileInfo(path).Length;
            }
            else if (Directory.Exists(path))
            {
                type = TargetType.Directory;
                size = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                                .Sum(f => new FileInfo(f).Length);
            }
            else if (Path.GetPathRoot(path)?.Equals(path, StringComparison.OrdinalIgnoreCase) == true)
            {
                type = TargetType.Drive;
                size = new DriveInfo(path).TotalSize;
            }
            else
            {
                type = TargetType.Unknown;
            }
        }
        catch
        {
            type = TargetType.Unknown;
        }

        return new TargetItem(path, type, ByteSizeConverter.ToHumanReadable(size));
    }

    private static string GetTypeName(string path)
    {
        if (File.Exists(path))
            return "[blue]File[/]";
        if (Directory.Exists(path))
            return "[yellow]Directory[/]";
        if (Path.GetPathRoot(path)?.Equals(path, StringComparison.OrdinalIgnoreCase) == true)
            return "[red]Drive[/]";
        return "[grey]Unknown[/]";
    }

    [GeneratedRegex("\"([^\"]+)\"|'([^']+)'|([^\\s;]+)")]
    private static partial Regex PathRegex();
}