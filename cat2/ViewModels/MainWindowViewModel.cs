using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using CAT2.Common;
using CommunityToolkit.Mvvm.Input;
using static CAT2.Common.Constants;
using Wpf.Ui.Appearance;

namespace CAT2.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        Init("CAT2");

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            WritingLog(
                $"请将此日志反馈给开发者\n1.QQ：2976779544\n版本号：{Constants.Version}次版本号：{FileVersion}\n异常信息：\n{(Exception)args.ExceptionObject}");
            Process.Start(new ProcessStartInfo
            {
                FileName = LogFilePath,
                UseShellExecute = true
            });
        };

        ApplicationThemeManager.Changed += (theme, _) => IsDarkTheme = theme == ApplicationTheme.Dark;

        if (File.Exists(SettingsFilePath)) return;
        File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(new
        {
            StartedItems = new Dictionary<string, bool>()
        }));
        WritingLog("settings.json文件不存在，已创建");
    }

#if DEBUG
    [ObservableProperty]
    private string _assemblyName = $"{Constants.AssemblyName} Dev";
#else
    [ObservableProperty] 
    private string _assemblyName = Constants.AssemblyName;
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