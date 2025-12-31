using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Appearance;

namespace CAT2.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        if (File.Exists(App.SettingsFilePath)) return;
        File.WriteAllText(App.SettingsFilePath, JsonSerializer.Serialize(new
        {
            StartedItems = new Dictionary<string, bool>()
        }));
    }

#if DEBUG
    [ObservableProperty]
    private string _assemblyName = $"{App.AssemblyName} Dev";
#else
    [ObservableProperty] 
    private string _assemblyName = App.AssemblyName;
#endif                        
    
    [ObservableProperty]
    private bool _isDarkTheme;

    [RelayCommand]
    private void ChangeTheme()
    {
        ApplicationThemeManager.Apply(ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark
            ? ApplicationTheme.Light
            : ApplicationTheme.Dark);
    }
}