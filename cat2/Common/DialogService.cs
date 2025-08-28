using System;
using Wpf.Ui;
using Wpf.Ui.Extensions;

namespace CAT2.Common;

public static class DialogService
{
    public static readonly SnackbarService SnackBarService = new();

    public static readonly ContentDialogService ContentDialogService = new();

    public static void ShowSnackBar
    (
        string title,
        string content,
        ControlAppearance appearance,
        SymbolRegular icon
    )
    {
        SnackBarService.Show(
            title,
            content,
            appearance,
            new SymbolIcon(icon) { FontSize = 35 },
            TimeSpan.FromSeconds(2));
    }

    public static async Task<bool> ShowConfirm
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
        ) == ContentDialogResult.Primary;
    }
}