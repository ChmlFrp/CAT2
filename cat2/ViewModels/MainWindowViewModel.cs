using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CAT2.Models;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
using static ChmlFrp.SDK.UserActions;

namespace CAT2.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
#if DEBUG
    [ObservableProperty] private string _assemblyName = $"{Constants.AssemblyName} Dev";
#else
    [ObservableProperty] private string _assemblyName = Constants.AssemblyName;
#endif

    [ObservableProperty] private bool _isDarkTheme;

    public MainWindowViewModel()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is not Exception ex) return;

            WritingLog(ex.Message.Contains("拒绝访问")
                ? "请以管理员身份运行本程序"
                : $"请将此日志反馈给开发者\n联系方式：\n1.QQ：2976779544\n2.Email：Qusay_Diaz@outlook.com\n3.GitHub：Qianyiaz/CAT2\n版本号：{Constants.Version}次版本号：{FileVersion}\n异常信息：\n{ex}");

            Process.Start(new ProcessStartInfo
            {
                FileName = LogFilePath,
                UseShellExecute = true
            });
        };

        MainClass.Loaded += async (_, _) =>
        {
            Init("CAT2");

            IsDarkTheme = ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark;
            await Task.WhenAll(
                Task.Run(() =>
                {
                    SnackbarService.SetSnackbarPresenter(MainClass.RootSnackbarDialog);
                    ContentDialogService.SetDialogHost(MainClass.RootContentDialogPresenter);
                }),
                LoginAsyncFromToken(),
                Task.Run(async () =>
                {
                    if (File.Exists(SettingsFilePath)) return;
                    await File.WriteAllTextAsync(SettingsFilePath, JsonSerializer.Serialize(new
                    {
                        StartedItems = new Dictionary<string, bool>()
                    }));
                    WritingLog("settings.json文件不存在，已创建");
                }));

            MainClass.RootNavigation.Navigate("登录");
            if (IsLoggedIn)
            {
                MainClass.LoginItem.Visibility = Visibility.Collapsed;
                MainClass.TunnelItem.Visibility = Visibility.Visible;
                MainClass.NodeItem.Visibility = Visibility.Visible;
                MainClass.UserItem.Visibility = Visibility.Visible;
                MainClass.RootNavigation.Navigate("管理隧道");
            }

            MainClass.Topmost = false;
            WritingLog("主窗口加载完成");
        };

        SystemEvents.UserPreferenceChanged += (_, _) =>
        {
            ApplicationThemeManager.Apply(ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light);
        };
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        ApplicationThemeManager.Apply(value ? ApplicationTheme.Dark : ApplicationTheme.Light);
    }
}