using CAT2.ViewModels;

namespace CAT2.Views.Pages;

public partial class NodePage
{
    public NodePage()
    {
        InitializeComponent();
        var viewModel = (NodePageViewModel)DataContext;
        Loaded += viewModel.Loaded;
    }
}