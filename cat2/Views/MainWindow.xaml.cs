using Wpf.Ui.Appearance;
using static System.Windows.Visibility;
using static ChmlFrp.SDK.UserActions;

namespace CAT2.Views;

public partial class MainWindow
{
    public MainWindow()
    {
        App.MainWindw = this;
        InitializeComponent();
        ApplicationThemeManager.ApplySystemTheme();
        SystemThemeWatcher.Watch(this);
        App.SnackBarService.SetSnackbarPresenter(RootSnackbarDialog);
        App.ContentDialogService.SetDialogHost(RootContentDialogPresenter);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (await AutoLoginAsync())
        {
            LoginItem.Visibility = Collapsed;
            TunnelItem.Visibility = Visible;
            NodeItem.Visibility = Visible;
            UserItem.Visibility = Visible; 
            RootNavigation.Navigate("用户信息");
        }
        else
        {
            RootNavigation.Navigate("登录");
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        RootNavigation.SetCurrentValue(NavigationView.IsPaneOpenProperty, e.NewSize.Width > 875);
    }
}