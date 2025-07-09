using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CAT2.ViewModels;

public partial class UserinfoPageViewModel : ObservableObject
{
    [ObservableProperty] private string _bandwidth;
    [ObservableProperty] private BitmapImage _currentImage;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _group;
    [ObservableProperty] private string _integral;
    [ObservableProperty] private bool _isFlyoutOpen;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _regtime;
    [ObservableProperty] private string _tunnelCount;

    public UserinfoPageViewModel()
    {
        Loading(null, null);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        timer.Tick += Loading;
        timer.Start();
    }

    [RelayCommand]
    private void OpenFlyout()
    {
        IsFlyoutOpen = true;
    }


    private async void Loading(object sender, EventArgs e)
    {
        var userInfo = await User.GetUserInfo();
        if (userInfo == null)
        {
            WritingLog("加载用户信息失败");
            ShowTip(
                "加载用户信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
            return;
        }

        Name = userInfo.username;
        Email = userInfo.email;
        Group = $"用户组：{userInfo.usergroup}";
        Integral = $"积分：{userInfo.integral}";
        Regtime = $"注册时间：{userInfo.regtime}";
        TunnelCount = $"隧道使用：{userInfo.tunnelCount}/{userInfo.tunnel}";
        Bandwidth = $"带宽限制：国内{userInfo.bandwidth}m | 国外{userInfo.bandwidth * 4}m";
        WritingLog("加载用户信息成功");

        if (CurrentImage != null) return;
        CurrentImage = new BitmapImage(new Uri(userInfo.userimg));
        WritingLog("用户头像加载成功");
    }

    [RelayCommand]
    private static async Task OnSignOut()
    {
        Sign.Signout();
        WritingLog("用户已退出登录");
        ShowTip(
            "已退出登录",
            "请重新登录以继续使用。",
            ControlAppearance.Info,
            SymbolRegular.SignOut24);
        await Task.Delay(1000);
        WritingLog("正在重启应用程序");
        Process.Start(Path.Combine(AppContext.BaseDirectory,
            Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName)!));
        Application.Current.Shutdown();
    }
}