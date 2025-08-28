namespace CAT2.Views.Pages;

public partial class NodePage
{
    public NodePage()
    {
        InitializeComponent();
        var vm = (NodePageViewModel)DataContext;
        Loaded += async (_, _) => await vm.Loaded();
    }
}