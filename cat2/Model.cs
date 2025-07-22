using System.IO;
using System.IO.Compression;
using System.Reflection;
using CAT2.Views;
using CSDK;
using Wpf.Ui;
using Wpf.Ui.Extensions;

namespace CAT2;

public abstract class Model
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

    public static readonly SnackbarService SnackbarService = new();

    public static readonly ContentDialogService ContentDialogService = new();

    public static void ShowTip(string title, string content, ControlAppearance appearance, SymbolRegular icon)
    {
        SnackbarService.Show(
            title,
            content,
            appearance,
            new SymbolIcon(icon) { FontSize = 32 },
            new TimeSpan(0, 0, 0, 2));
    }

    public static async Task<ContentDialogResult> ShowConfirm(string title, string content,
        string primaryButtonText = "确定", string closeButtonText = "取消")
    {
        return await ContentDialogService.ShowSimpleDialogAsync(
            new SimpleContentDialogCreateOptions
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText
            }
        );
    }

    public static async Task UpdateApp(bool showTip = false)
    {
        var jsonNode = await Http.GetJsonAsync("https://cat2.chmlfrp.com/update.json");
        if (jsonNode == null)
        {
            if (showTip)
                ShowTip("更新失败",
                    "无法获取更新信息，请稍后再试。",
                    ControlAppearance.Danger,
                    SymbolRegular.TagError24);
            return;
        }

        if ((string)jsonNode["version"] == Version)
        {
            if (showTip)
                ShowTip("已是最新版本",
                    "当前应用已是最新版本，无需更新。",
                    ControlAppearance.Success,
                    SymbolRegular.TagError24);
            return;
        }

        ShowTip("发现新版本",
            "正在检查更新...",
            ControlAppearance.Light,
            SymbolRegular.Add48);

        await Task.Delay(1000);

        if (await ShowConfirm("更新确认",
                $"当前版本：{Version}\n最新版本：{jsonNode["version"]}\n\n是否立即更新？",
                "更新") != ContentDialogResult.Primary)
            return;

        ShowTip("正在下载更新",
            "请稍候，正在下载最新版本...",
            ControlAppearance.Success,
            SymbolRegular.Add48);

        var temp = Path.GetTempFileName();
        if (!await Http.GetFileAsync("https://gitcode.com/Qyzgj/cat2/releases/download/lastest/Release.zip",
                temp)) return;

        try
        {
            ZipFile.ExtractToDirectory(temp, DataPath);
        }
        catch
        {
            if (showTip)
                ShowTip("更新失败",
                    "无法解压更新文件，请检查磁盘空间或权限。",
                    ControlAppearance.Danger,
                    SymbolRegular.TagError24);
            return;
        }
        finally
        {
            File.Delete(temp);
        }

        Process.Start(
            new ProcessStartInfo
            (
                "cmd.exe",
                $"""/c %WINDIR%\System32\timeout.exe /t 3 /nobreak & move /y "{Path.Combine(DataPath, $"{AssemblyName}.exe")}" "{Process.GetCurrentProcess().MainModule?.FileName}" & start "" "{Process.GetCurrentProcess().MainModule?.FileName}" """
            )
            {
                UseShellExecute = false,
                CreateNoWindow = true
            });
        Application.Current.Shutdown();
    }
}