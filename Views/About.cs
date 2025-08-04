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
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        string versionString = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "Unknown";
        table.AddRow("[yellow]Version[/]", $"v{versionString}");
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

        var donatePanel = new Panel(new Markup(
            "\n:red_heart:  [bold yellow]Support this project[/]\n" +
            "[grey]BTC:[/] 1A1pm2DNMFtsBHzKea64HCogLKMpScagZu\n" +
            "[grey]XMR:[/] 897p7tTp8BRGUY92XYvUcCABmWfMpSQqa2APFK28wwHsC6b3RaUVPQ1EYyJ66jqLhxWdBLe9FvcHoAEn7K2PYhrAS7SJKGw"
        ))
        {
            Border = BoxBorder.None,
            Padding = new Padding(1, 0),
        };

        AnsiConsole.Write(donatePanel);

        AnsiConsole.WriteLine();
    }
}