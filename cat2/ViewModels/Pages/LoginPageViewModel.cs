using static ChmlFrp.SDK.UserActions;

namespace CAT2.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty] private bool _isLoggedInEnabled;

    [ObservableProperty] private string _password;
    [ObservableProperty] private string _username;

    partial void OnUsernameChanged(string value)
    {
        IsLoggedInEnabled = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(Password);
    }

    partial void OnPasswordChanged(string value)
    {
        IsLoggedInEnabled = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(Username);
    }

    [RelayCommand]
    private async Task LoggedIn()
    {
        var msg = await LoginAsync(Username, Password);
        WritingLog($"登录提示：{msg}");

        if (IsLoggedIn)
        {
            ShowSnackBar(
                "登录成功！",
                $"欢迎回来，{Username}！",
                ControlAppearance.Success,
                SymbolRegular.PresenceAvailable24);
        }
        else if (msg == null)
        {
            ShowSnackBar(
                "登录错误",
                "网络错误，请稍后再试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
        else
        {
            ShowSnackBar(
                "登录错误",
                $"{msg}",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
    }

    [RelayCommand]
    private async Task RegisterClick()
    {
        ShowSnackBar(
            "跳转至网页中...",
            "请稍等...",
            ControlAppearance.Info,
            SymbolRegular.Tag24);

        await Task.Delay(500);
        Register();
    }
}