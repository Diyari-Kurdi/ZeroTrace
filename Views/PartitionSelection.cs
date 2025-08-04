using Spectre.Console;
using System.Runtime.InteropServices;
using ZeroTrace.Enums;
using ZeroTrace.Helpers;
using ZeroTrace.Model;

namespace ZeroTrace.Views;

public static partial class PartitionSelection
{
    public static void Show()
    {
        while (true)
        {
            var target = AskForTarget();

            if (target is null)
            {
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title("[green]Selected Targets[/]")
                .AddColumn("Type")
                .AddColumn("Path")
                .AddColumn("Size");

            var type = new Markup($"[red]Drive[/]");
            var path = new TextPath(target.Path)
                .LeafColor(Color.Yellow)
                .SeparatorColor(Color.Green)
                .RootColor(Color.Red)
                .StemColor(Color.Blue);
            var size = new Text(target.Size);

            table.AddRow(type, path, size);

            AnsiConsole.Write(table);
            var result = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .AddChoices("Clear", "Continue"));


            if (result.StartsWith("Clear"))
            {
                target = null;
            }
            else if (result.StartsWith("Continue"))
            {
                var confirmation = AnsiConsole.Prompt(
                    new TextPrompt<bool>("[bold red]Are you sure you want to securely delete the selected target?[/]")
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(false)
                        .WithConverter(choice => choice ? "y" : "n"));
                AnsiConsole.WriteLine();
                if (confirmation)
                {
                    DriveInfo driveInfo = new(target.Path);
                    Exception? partitionException = null;
                    AnsiConsole.Progress()
                        .Start(ctx =>
                        {
                            var task1 = ctx.AddTask($"[green]{ByteSizeConverter.ToHumanReadable(driveInfo.TotalSize)} space to fill[/]", true, driveInfo.TotalSize);

                            PartitionDeleteService.ProgressChanged += () =>
                            {
                                task1.Value(driveInfo.TotalSize - driveInfo.TotalFreeSpace);
                            };
                            PartitionDeleteService.Completed += () =>
                            {
                                task1.Value(driveInfo.TotalSize);
                            };

                            try
                            {
                                PartitionDeleteService.FillPartitionWithTempData(target.Path);
                            }
                            catch (Exception ex)
                            {
                                partitionException = ex;
                                return;
                            }
                        });

                    if (partitionException is not null)
                    {
                        AnsiConsole.Prompt(
                            new TextPrompt<string>($"[red]An error occurred while filling the partition: {partitionException.Message}[/]\n[red bold]Press Enter...[/]")
                            .DefaultValue(string.Empty)
                            .HideDefaultValue());
                        CleanUp();
                        continue;
                    }

                    Exception? fileException = null;

                    AnsiConsole.Progress()
                       .Start(ctx =>
                       {
                           var files = FileTargetHelper.GetFiles([new TargetItem(driveInfo.Name, TargetType.Drive, ByteSizeConverter.ToHumanReadable(driveInfo.TotalSize))]);
                           var passes = 7;

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
                           try
                           {
                               FileDeleteService.Delete(files.AsReadOnly(), passes);
                               FileTargetHelper.CleanUpDirectories(driveInfo.RootDirectory.Name);
                           }
                           catch (Exception ex)
                           {
                               fileException = ex;
                               return;
                           }
                       });

                    if (fileException is not null)
                    {
                        AnsiConsole.Prompt(
                            new TextPrompt<string>($"[red]An error occurred: {fileException.Message}[/]\n[red bold]Press Enter...[/]")
                            .DefaultValue(string.Empty)
                            .HideDefaultValue());
                        CleanUp();
                        continue;
                    }

                    AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold green]Deletation completed![/] Press Enter... ")
                    .DefaultValue(string.Empty)
                    .HideDefaultValue());
                    target = null;

                }
            }
            CleanUp();
        }

    }

    private static void CleanUp()
    {
        AnsiConsole.Clear();
        About.Show();
    }

    private static TargetItem? AskForTarget()
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
                return new UserChoise($":computer_disk: {d.Name} ({label} - {size})", new TargetItem(d.Name, TargetType.Drive, size));
            })
            .ToList();

        choices.Add(new(":back_arrow: Go Back", null));


        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<UserChoise>()
                .Title("[bold red]Choose target to securely delete:[/]")
                .AddChoices(choices).UseConverter(p => p.DisplayText));

        if (selected.DisplayText.StartsWith(":back_arrow:"))
        {
            return null;
        }

        return selected.Item;
    }
}
