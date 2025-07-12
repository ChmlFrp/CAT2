using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Threading;

namespace CAT2.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
    [ObservableProperty] private string _assemblyName = Model.AssemblyName;
    [ObservableProperty] private string _context = IsFrpcExists ? "已下载" : "下载中";
    [ObservableProperty] private string _copyright = Model.Copyright;
    [ObservableProperty] private string _fileVersion = $"文件版本：{Model.FileVersion}";
    [ObservableProperty] private bool _isAutoUpdateEnabled;
    [ObservableProperty] private string _version = Model.Version;

    public SettingPageViewModel()
    {
        var data = File.ReadAllText(SettingsFilePath);
        var deserialize = JsonSerializer.Deserialize<Dictionary<string, bool>>(data);
        IsAutoUpdateEnabled = deserialize["IsAutoUpdate"];

        var dotCount = 0;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) =>
        {
            dotCount = (dotCount + 1) % 4;
            Context = "下载中" + new string('.', dotCount);
            if (!IsFrpcExists) return;
            Context = "已下载";
            timer.Stop();
        };
        timer.Start();
    }

    [RelayCommand]
    private void OpenDataPath()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = DataPath,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private void ClearCache()
    {
        foreach (var cachefile in Directory.GetFiles(DataPath, "*.log"))
            try
            {
                File.Delete(cachefile);
            }
            catch
            {
                // ignored
            }

        ShowTip(
            "缓存已清理",
            "所有缓存文件已被删除。",
            ControlAppearance.Success,
            SymbolRegular.PresenceAvailable24);
    }

    [RelayCommand]
    private void AutoUpdate()
    {
        if (IsAutoUpdateEnabled)
        {
            ShowTip(
                "自动更新已启用",
                "应用将在下次启动时自动检查更新。",
                ControlAppearance.Success,
                SymbolRegular.CheckmarkCircle24);
            File.WriteAllText(SettingsFilePath,
                JsonSerializer.Serialize(new Dictionary<string, bool> { { "IsAutoUpdate", true } }));
        }
        else
        {
            ShowTip(
                "自动更新已禁用",
                "应用将不会自动检查更新，请手动检查。",
                ControlAppearance.Success,
                SymbolRegular.CheckmarkCircle24);
            File.WriteAllText(SettingsFilePath,
                JsonSerializer.Serialize(new Dictionary<string, bool> { { "IsAutoUpdate", false } }));
        }
    }

    [RelayCommand]
    private static void Update()
    {
        UpdateApp(true);
    }
}