using static CSDK.UserActions;

namespace CAT2.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty] private string _username;
    partial void OnUsernameChanged(string value)
    {
        IsLoggedInEnabled = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(Password);
    }

    [ObservableProperty] private string _password;
    partial void OnPasswordChanged(string value)
    {
        IsLoggedInEnabled = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(Username);
    }

    [ObservableProperty] private bool _isLoggedInEnabled;
    [RelayCommand]
    private async Task LoggedIn()
    {
        var msg = await LoginAsync(Username, Password);
        WritingLog($"登录提示：{msg}");

        if (IsLoggedIn)
        {
            MainClass.LoginItem.Visibility = Visibility.Collapsed;
            MainClass.UserItem.Visibility = Visibility.Visible;
            MainClass.TunnelItem.Visibility = Visibility.Visible;
            MainClass.NodeItem.Visibility = Visibility.Visible;
            MainClass.RootNavigation.Navigate("用户信息");
            ShowSnackbar(
                "登录成功！",
                $"欢迎回来，{Username}！",
                ControlAppearance.Success,
                SymbolRegular.PresenceAvailable24);
        }
        else if (msg == null)
        {
            ShowSnackbar(
                "登录错误",
                "网络错误，请稍后再试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
        else
        {
            ShowSnackbar(
                "登录错误",
                $"{msg}",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
    }

    [RelayCommand]
    private async Task RegisterClick()
    {
        ShowSnackbar(
            "跳转至网页中...",
            "请稍等...",
            ControlAppearance.Info,
            SymbolRegular.Tag24);

        await Task.Delay(500);
        Register();
    }
}