using Spectre.Console;
using System.Reflection;

namespace ZeroTrace.Views;

public static class About
{
    public static void Show()
    {
        AnsiConsole.Write(
            new FigletText("ZeroTrace")
                .Centered()
                .Color(Color.Red));

        var panel = new Panel(new Markup("[bold]Secure File Deletion Tool[/]\n\n" +
                             "Safely overwrites and deletes files using custom patterns like zero fill, " +
                             "random data, byte shaking, and more.").LeftJustified())
        {
            Border = BoxBorder.Square,
            Padding = new Padding(2, 1),
            Header = new PanelHeader("About", Justify.Center)
        }.Expand();

        var table = new Table { Border = TableBorder.HeavyEdge };
        table.AddColumn(new TableColumn("Key").LeftAligned());
        table.AddColumn(new TableColumn("Value").LeftAligned());
        table.HideHeaders();
        table.AddRow("[yellow]Author[/]", "Diyari Ismael");
        table.AddRow("[yellow]GitHub[/]", "[link=https://github.com/Diyari-Kurdi/ZeroTrace]https://github.com/Diyari-Kurdi/ZeroTrace[/]");
        table.AddRow("[yellow]Version[/]", "v1.0.0");
        table.AddRow("[yellow]License[/]", "MIT");

        var mainGrid = new Grid();
        mainGrid.AddColumn();
        mainGrid.AddColumn();

        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "ZeroTrace.shred16.png";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            var image = new CanvasImage(stream);
            image.NoMaxWidth();
            mainGrid.AddRow(image, new Rows(panel.Expand(), table.Expand()));
        }
        else
        {
            mainGrid.AddRow(new Rows(panel.Expand(), table.Expand()));
        }

        AnsiConsole.Write(mainGrid);

        AnsiConsole.WriteLine();
    }
}