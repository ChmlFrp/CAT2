namespace CAT2.Views.Pages;

public partial class TunnelPage
{
    public TunnelPage()
    {
        InitializeComponent();
        var vm = (TunnelPageViewModel)DataContext;
        Loaded += async (_, _) => await vm.Loaded();
    }
}