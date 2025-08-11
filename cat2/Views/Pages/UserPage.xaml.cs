namespace CAT2.Views.Pages;

public partial class UserPage
{
    public UserPage()
    {
        InitializeComponent();
        var viewModel = (UserPageViewModel)DataContext;
        Loaded += viewModel.Loaded;
    }
}