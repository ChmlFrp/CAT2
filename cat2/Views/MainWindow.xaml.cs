using CAT2.ViewModels;

namespace CAT2.Views;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = (MainWindowViewModel)DataContext;
        Loaded += viewModel.Loaded;
    }
}