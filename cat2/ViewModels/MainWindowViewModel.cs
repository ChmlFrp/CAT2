﻿using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CAT2.Models;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
using static CSDK.UserActions;

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

        MainClass.Loaded += async (_, _) =>
        {
            Init("CAT2");

            if (ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark) ThemesChanged();

            SnackbarService.SetSnackbarPresenter(MainClass.RootSnackbarDialog);
            ContentDialogService.SetDialogHost(MainClass.RootContentDialogPresenter);
            MainClass.RootNavigation.Navigate("登录");
            await AutoLoginAsync(); // 尝试自动登录
            if (IsLoggedIn)
            {
                MainClass.LoginItem.Visibility = Visibility.Collapsed;
                MainClass.UserItem.Visibility = Visibility.Visible;
                MainClass.TunnelItem.Visibility = Visibility.Visible;
                MainClass.RootNavigation.Navigate("用户页");
            }

            MainClass.Topmost = false;

            if (!File.Exists(SettingsFilePath))
            {
                await File.WriteAllTextAsync(SettingsFilePath,
                    JsonSerializer.Serialize(new Dictionary<string, bool> { { "IsAutoUpdate", true } }));
                WritingLog("settings.json文件不存在，已创建");
            }

            var data = File.ReadAllText(SettingsFilePath);
            var deserialize = JsonSerializer.Deserialize<Dictionary<string, bool>>(data);

            if (deserialize["IsAutoUpdate"])
            {
                WritingLog("自动更新已启用");
                await UpdateApp();
            }
            else
            {
                WritingLog("自动更新已禁用");
            }

            WritingLog("主窗口加载完成");
            await SetFrpcAsync();
        };
    }

    [RelayCommand]
    private void ThemesChanged()
    {
        var theme = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light;
        ApplicationThemeManager.Apply(theme ? ApplicationTheme.Dark : ApplicationTheme.Light);
        IsDarkTheme = theme;
    }
}