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
    [ObservableProperty] private string _assemblyName = Constants.AssemblyName;
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

        SystemEvents.UserPreferenceChanged += (_, _) =>
        {
            var theme = ApplicationThemeManager.GetSystemTheme() == SystemTheme.Light;
            ApplicationThemeManager.Apply(theme ? ApplicationTheme.Light : ApplicationTheme.Dark);
            IsDarkTheme = !theme;
        };
    }

    public async void Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            Init("CAT2");

            if (ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark) ThemesChanged();

            var uiSetupTask = Task.Run(() =>
            {
                SnackbarService.SetSnackbarPresenter(MainClass.RootSnackbarDialog);
                ContentDialogService.SetDialogHost(MainClass.RootContentDialogPresenter);
            });

            MainClass.RootNavigation.Navigate("登录");

            await Task.WhenAll(uiSetupTask, LoginAsyncFromToken(), CheckAndCreateSettingsFileAsync());

            UpdateUiForLoggedInUser();

            MainClass.Topmost = false;
            WritingLog("主窗口加载完成");
        }
        catch (Exception ex)
        {
            WritingLog($"加载过程中发生错误: {ex.Message}");
        }
    }

    private async Task CheckAndCreateSettingsFileAsync()
    {
        if (!File.Exists(SettingsFilePath))
        {
            var defaultSettings = new Dictionary<string, bool> { { "IsAutoUpdate", true } };
            await File.WriteAllTextAsync(SettingsFilePath, JsonSerializer.Serialize(defaultSettings));
            WritingLog("settings.json文件不存在，已创建");
        }
    }

    private void UpdateUiForLoggedInUser()
    {
        if (!IsLoggedIn) return;
        MainClass.LoginItem.Visibility = Visibility.Collapsed;
        MainClass.UserItem.Visibility = Visibility.Visible;
        MainClass.TunnelItem.Visibility = Visibility.Visible;
        MainClass.NodeItem.Visibility = Visibility.Visible;
        MainClass.RootNavigation.Navigate("用户信息");
    }

    [RelayCommand]
    private void ThemesChanged()
    {
        var theme = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light;
        ApplicationThemeManager.Apply(theme ? ApplicationTheme.Dark : ApplicationTheme.Light);
        IsDarkTheme = theme;
    }
}