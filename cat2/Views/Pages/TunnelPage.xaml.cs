namespace CAT2.Views.Pages;

public partial class TunnelPage
{
    public TunnelPage()
    {
        InitializeComponent();
        var viewModel = (TunnelPageViewModel)DataContext;
        Loaded += viewModel.Loaded;
    }
}