using Avalonia;
using AvaloniaTest;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace SyncMP3App;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        NativeMethods.AllocConsole();
        Console.WriteLine("Console attached");
        Console.WriteLine("Hello World");
        Console.WriteLine(Path.Combine(AppContext.BaseDirectory, "Data"));
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}

static class NativeMethods
{
    [DllImport("kernel32.dll")]
    internal static extern bool AllocConsole();
}
