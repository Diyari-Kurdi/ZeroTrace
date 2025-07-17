using Spectre.Console;

namespace ZeroTrace.Views;

public static class Selection
{
    public static void Show()
    {
        while (true)
        {
            var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Select target type")
                .AddChoices(":computer_disk: Partition", ":open_file_folder: Files", ":cross_mark: Exit"));

            if (selection.StartsWith(":computer_disk:"))
            {
                PartitionSelection.Show();
            }
            else if (selection.StartsWith(":open_file_folder:"))
            {
                FileSelection.Show();
            }
            else
            {
                return;
            }
        }
    }
}
