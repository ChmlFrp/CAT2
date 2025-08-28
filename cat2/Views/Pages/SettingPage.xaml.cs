namespace CAT2.Views.Pages;

public partial class SettingPage
{
    public SettingPage()
    {
        InitializeComponent();
        var vm = (SettingPageViewModel)DataContext;
        Loaded += async (_, _) => await vm.Loaded();
    }
}