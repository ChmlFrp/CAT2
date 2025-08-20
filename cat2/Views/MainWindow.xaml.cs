using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CAT2.Models;
using Wpf.Ui.Appearance;
using static ChmlFrp.SDK.UserActions;

namespace CAT2.Views;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var ex = (Exception)args.ExceptionObject;
            WritingLog($"请将此日志反馈给开发者\n联系方式：\n1.QQ：2976779544\n2.Email：Qusay_Diaz@outlook.com\n3.GitHub：Qianyiaz/CAT2\n版本号：{Constants.Version}次版本号：{FileVersion}\n异常信息：\n{ex}");
            Process.Start(new ProcessStartInfo
            {
                FileName = LogFilePath,
                UseShellExecute = true
            });
        };

        ApplicationThemeManager.Changed += (theme, _) =>
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.IsDarkTheme = theme == ApplicationTheme.Dark;
        };
        ApplicationThemeManager.ApplySystemTheme();
        SystemThemeWatcher.Watch(this);

        SizeChanged += (_, args) =>
            RootNavigation.SetCurrentValue(NavigationView.IsPaneOpenProperty, args.NewSize.Width > 875);

        Init("CAT2");

        SnackBarService.SetSnackbarPresenter(RootSnackbarDialog);
        ContentDialogService.SetDialogHost(RootContentDialogPresenter);
        if (!File.Exists(SettingsFilePath))
        {
            await File.WriteAllTextAsync(SettingsFilePath, JsonSerializer.Serialize(new
            {
                StartedItems = new Dictionary<string, bool>()
            }));
            WritingLog("settings.json文件不存在，已创建");
        }

        var first = true;
        OnIsLoggedInChange += value =>
        {
            if (value)
            {
                LoginItem.SetValue(VisibilityProperty, Visibility.Collapsed);
                TunnelItem.SetValue(VisibilityProperty, Visibility.Visible);
                NodeItem.SetValue(VisibilityProperty, Visibility.Visible);
                UserItem.SetValue(VisibilityProperty, Visibility.Visible);

                if (first)
                    RootNavigation.Navigate("管理隧道");
                first = false;
            }
            else
            {
                LoginItem.SetValue(VisibilityProperty, Visibility.Visible);
                TunnelItem.SetValue(VisibilityProperty, Visibility.Collapsed);
                NodeItem.SetValue(VisibilityProperty, Visibility.Collapsed);
                UserItem.SetValue(VisibilityProperty, Visibility.Collapsed);

                RootNavigation.Navigate("登录");
                first = true;
            }
        };
        await AutoLoginAsync();
        WritingLog("主窗口加载完成");
    }
}