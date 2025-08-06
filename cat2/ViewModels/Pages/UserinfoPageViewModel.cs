using System.Windows.Media.Imaging;
using static ChmlFrp.SDK.UserActions;

namespace CAT2.ViewModels;

public partial class UserinfoPageViewModel : ObservableObject
{
    // 用户信息
    [ObservableProperty] private string _bandwidth;
    [ObservableProperty] private BitmapImage _currentImage;
    [ObservableProperty] private string _email;
    private bool _first = true;
    [ObservableProperty] private string _group;
    [ObservableProperty] private string _integral;

    [ObservableProperty] private bool _isLoadedEnabled = true;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _qq;
    [ObservableProperty] private string _term;
    [ObservableProperty] private string _tunnelCount;

    [RelayCommand]
    private void Loaded()
    {
        Loaded(null, null);
    }

    public async void Loaded(object sender, RoutedEventArgs e)
    {
        IsLoadedEnabled = false;

        if (_first)
            _first = false;
        else
            await LoginAsyncFromToken();

        if (UserInfo == null)
        {
            WritingLog("加载用户信息失败");
            ShowSnackbar(
                "加载用户信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
            IsLoadedEnabled = true;
            return;
        }

        Name = $"Hi,{UserInfo.username}👋";
        Email = UserInfo.email;
        Group = $"用户组：{UserInfo.usergroup}";
        Integral = $"积分：{UserInfo.integral}";
        Term = $"到期时间：{UserInfo.term}";
        Qq = $"QQ：{UserInfo.qq}";
        TunnelCount = $"隧道使用：{UserInfo.tunnelCount}/{UserInfo.tunnel}";
        Bandwidth = $"带宽限制：国内{UserInfo.bandwidth}m | 国外{UserInfo.bandwidth * 4}m";
        WritingLog("加载用户信息成功");

        IsLoadedEnabled = true;

        if (CurrentImage != null) return;
        CurrentImage = new BitmapImage(new Uri(UserInfo.userimg));
        WritingLog("用户头像加载成功");
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