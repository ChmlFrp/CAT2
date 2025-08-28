namespace CAT2.Views.Pages;

public partial class UserPage
{
    public UserPage()
    {
        InitializeComponent();
        var vm = (UserPageViewModel)DataContext;
        Loaded += async (_, _) => await vm.Loaded();
    }
}