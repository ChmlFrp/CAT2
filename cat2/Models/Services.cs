using Wpf.Ui;
using Wpf.Ui.Extensions;

namespace CAT2.Models;

public static class Services
{
    public static readonly SnackbarService SnackbarService = new();

    public static readonly ContentDialogService ContentDialogService = new();

    public static void ShowSnackbar
    (
        string title,
        string content,
        ControlAppearance appearance,
        SymbolRegular icon
    )
    {
        SnackbarService.Show(
            title,
            content,
            appearance,
            new SymbolIcon(icon) { FontSize = 32 },
            new TimeSpan(0, 0, 0, 2));
    }

    public static async Task<ContentDialogResult> ShowConfirm
    (
        string title,
        string content,
        string primaryButtonText = "确定",
        string closeButtonText = "取消"
    )
    {
        return await ContentDialogService.ShowSimpleDialogAsync(
            new SimpleContentDialogCreateOptions
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText
            }
        );
    }
}