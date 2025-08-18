using CAT2.Models;
using Wpf.Ui.Appearance;

namespace CAT2.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
#if DEBUG
    [ObservableProperty] private string _assemblyName = $"{Constants.AssemblyName} Dev";
#else
    [ObservableProperty] private string _assemblyName = Constants.AssemblyName;
#endif

    [ObservableProperty] private bool _isDarkTheme;

    [RelayCommand]
    private void ChangeTheme()
    {
        ApplicationThemeManager.Apply(ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark
            ? ApplicationTheme.Light
            : ApplicationTheme.Dark);
    }
}