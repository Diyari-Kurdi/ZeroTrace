using Spectre.Console;
using ZeroTrace.Views;

namespace ZeroTrace;

internal class Program
{
    static void Main(string[] args)
    {
        About.Show();

        AnsiConsole.Markup("Press [green]Enter[/] to exit...");
        Console.ReadLine();
    }
}
