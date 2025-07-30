using CAT2.ViewModels;

namespace CAT2.Views.Pages;

public partial class UserinfoPage
{
    public UserinfoPage()
    {
        InitializeComponent();
        var viewModel = (UserinfoPageViewModel)DataContext;
        Loaded += viewModel.Loaded;
    }
}