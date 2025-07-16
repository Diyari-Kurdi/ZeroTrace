using Spectre.Console;
using ZeroTrace.Views;

namespace ZeroTrace;

internal class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        //About.Show();
        TargetSelection.Show();

        AnsiConsole.Markup("Press [green]Enter[/] to exit...");
        Console.ReadLine();
    }
}