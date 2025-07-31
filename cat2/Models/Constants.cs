using System.IO;
using System.Reflection;
using CAT2.Views;

namespace CAT2.Models;

public static class Constants
{
    public static readonly MainWindow MainClass = (MainWindow)Application.Current.MainWindow;

    public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    public static readonly string FileVersion = Assembly
        .GetExecutingAssembly()
        .GetCustomAttribute<AssemblyFileVersionAttribute>()?
        .Version;

    public static readonly string Copyright = Assembly
        .GetExecutingAssembly()
        .GetCustomAttribute<AssemblyCopyrightAttribute>()?
        .Copyright;

    public static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    public static readonly string SettingsFilePath = Path.Combine(DataPath, "Settings-CAT2.json");
}