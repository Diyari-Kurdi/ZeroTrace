using Spectre.Console;
using ZeroTrace.Enums;
using ZeroTrace.Helpers;
using ZeroTrace.Model;
using ZeroTrace.Services;

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
                    AnsiConsole.Progress()
                        .Start(ctx =>
                        {
                            var passes = 7;
                            var task1 = ctx.AddTask($"[green]{passes} passes to complete[/]", true, passes);

                            DeleteService.PassCompleted += (pass) =>
                            {
                                task1.Value(pass);
                            };

                            DeleteService.DeletePartition(target.Path, passes);
                        });

                    AnsiConsole.Prompt(
                        new TextPrompt<string>("[bold green]Deletation completed![/] Press Enter... ")
                        .DefaultValue(string.Empty)
                        .HideDefaultValue());

                    target = null;
                }
            }
            AnsiConsole.Clear();
        }

    }

    private static TargetItem? AskForTarget()
    {
        var systemRoot = Path.GetPathRoot(Environment.SystemDirectory);

        var drives = DriveInfo.GetDrives()
            .Where(d => d.DriveType == DriveType.Removable || d.DriveType == DriveType.Fixed
                && d.IsReady
                && !d.Name.Equals(systemRoot, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var choices = drives
            .Select(d =>
            {
                string label = string.IsNullOrWhiteSpace(d.VolumeLabel) ? "No Label" : d.VolumeLabel;
                string size = ByteSizeConverter.ToHumanReadable(d.TotalSize);
                return new UserChoise($":computer_disk: {d.Name} ({label} - {size})", new TargetItem(d.Name, TargetType.Drive, size));
            })
            .ToList();

        choices.Add(new(":cross_mark: Exit", null));


        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<UserChoise>()
                .Title("[bold red]Choose target to securely delete:[/]")
                .AddChoices(choices).UseConverter(p => p.DisplayText));

        if (selected.DisplayText.StartsWith(":cross_mark:"))
        {
            return null;
        }

        return selected.Item;
    }
}
