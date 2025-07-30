using CAT2.ViewModels;

namespace CAT2.Views.Pages;

public partial class SettingPage
{
    public SettingPage()
    {
        InitializeComponent();
        var viewModel = (SettingPageViewModel)DataContext;
        Loaded += viewModel.Loaded;
    }
}