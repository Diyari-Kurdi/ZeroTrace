using ZeroTrace.Views;

namespace ZeroTrace;

internal class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        About.Show();
        Selection.Show();
    }
}