using System.Windows.Media.Imaging;
using static ChmlFrp.SDK.UserActions;

namespace CAT2.ViewModels;

public partial class UserPageViewModel : ObservableObject
{
    [ObservableProperty] private BitmapImage _currentImage;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _usergroup;
    [ObservableProperty] private string _qq;
    [ObservableProperty] private string _integral;
    [ObservableProperty] private string _bandwidth;
    [ObservableProperty] private string _tunnelCount;
    [ObservableProperty] private bool _isLoadedEnabled;

    public async void Loaded(object sender, RoutedEventArgs e)
    {
        await Loaded();
    }

    private bool _first = true;
    [RelayCommand]
    private async Task Loaded()
    {
        IsLoadedEnabled = false;
        if (_first)
        {
            CurrentImage = new BitmapImage(new(UserInfo.userimg));
            _first = false;
        }
        else
        {
            await LoginAsyncFromToken();
        }

        Name = UserInfo.username;
        Email = UserInfo.email;
        Usergroup = $"用户组：{UserInfo.usergroup}";
        Name = $"Hi,{UserInfo.username}👋";
        Integral = $"积分：{UserInfo.integral}";
        Qq = $"QQ：{UserInfo.qq}";
        TunnelCount = $"隧道使用：{UserInfo.tunnelCount}/{UserInfo.tunnel}";
        Bandwidth = $"带宽限制：国内{UserInfo.bandwidth}m | 国外{UserInfo.bandwidth * 4}m";
        IsLoadedEnabled = true;
    }
    
    [RelayCommand]
    private static async Task OnSignOut()
    {
        if (await ShowConfirm(
                "你确定要退出登录吗?",
                "退出登录后你的用户Token将删除。",
                "确认",
                "放弃") != ContentDialogResult.Primary) return;
        Logout();

        WritingLog("用户已退出登录");
        ShowSnackbar(
            "已退出登录",
            "请重新登录以继续使用。",
            ControlAppearance.Info,
            SymbolRegular.SignOut24);

        MainClass.LoginItem.Visibility = Visibility.Visible;
        MainClass.TunnelItem.Visibility = Visibility.Collapsed;
        MainClass.NodeItem.Visibility = Visibility.Collapsed;
        MainClass.UserItem.Visibility = Visibility.Collapsed;
        MainClass.RootNavigation.Navigate("登录");
    }
}