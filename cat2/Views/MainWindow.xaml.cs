using Wpf.Ui.Appearance;
using static System.Windows.Visibility;
using static ChmlFrp.SDK.UserActions;

namespace CAT2.Views;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        ApplicationThemeManager.ApplySystemTheme();
        SystemThemeWatcher.Watch(this);
        SnackBarService.SetSnackbarPresenter(RootSnackbarDialog);
        ContentDialogService.SetDialogHost(RootContentDialogPresenter);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var first = true;
        OnIsLoggedInChange += value =>
        {
            if (value)
            {
                LoginItem.Visibility = Collapsed;
                TunnelItem.Visibility = Visible;
                NodeItem.Visibility = Visible;
                UserItem.Visibility = Visible;

                if (first)
                    RootNavigation.Navigate("管理隧道");
                first = false;
            }
            else
            {
                LoginItem.Visibility = Visible;
                TunnelItem.Visibility = Collapsed;
                NodeItem.Visibility = Collapsed;
                UserItem.Visibility = Collapsed;

                RootNavigation.Navigate("登录");
                first = true;
            }
        };

        await AutoLoginAsync();
        WritingLog("主窗口加载完成");
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        RootNavigation.SetCurrentValue(NavigationView.IsPaneOpenProperty, e.NewSize.Width > 875);
    }
}