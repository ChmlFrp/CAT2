using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Threading;
using CAT2.Models;

namespace CAT2.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
    [ObservableProperty] private string _assemblyName = Constants.AssemblyName;
    [ObservableProperty] private string _context = IsFrpcExists ? "已下载" : "下载中";
    [ObservableProperty] private string _copyright = Constants.Copyright;
    [ObservableProperty] private string _fileVersion = $"文件版本：{Constants.FileVersion}";
    [ObservableProperty] private bool _isAutoUpdateEnabled;

    [ObservableProperty] private bool _isClearCacheButtonEnabled = true;
    [ObservableProperty] private bool _isUpdateButtonEnabled = true;
    [ObservableProperty] private string _version = Constants.Version;


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

    async partial void OnIsAutoUpdateEnabledChanged(bool value)
    {
        if (value)
        {
            ShowSnackbar(
                "自动更新已启用",
                "应用将在下次启动时自动检查更新。",
                ControlAppearance.Success,
                SymbolRegular.CheckmarkCircle24);
            await File.WriteAllTextAsync(SettingsFilePath,
                JsonSerializer.Serialize(new Dictionary<string, bool> { { "IsAutoUpdate", true } }));
        }
        else
        {
            ShowSnackbar(
                "自动更新已禁用",
                "应用将不会自动检查更新，请手动检查。",
                ControlAppearance.Success,
                SymbolRegular.CheckmarkCircle24);
            await File.WriteAllTextAsync(SettingsFilePath,
                JsonSerializer.Serialize(new Dictionary<string, bool> { { "IsAutoUpdate", false } }));
        }
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
        IsClearCacheButtonEnabled = false;
        foreach (var cacheFile in Directory.GetFiles(DataPath, "*.log"))
            try
            {
                File.Delete(cacheFile);
            }
            catch
            {
                // ignored
            }

        ShowSnackbar(
            "缓存已清理",
            "所有缓存文件已被删除。",
            ControlAppearance.Success,
            SymbolRegular.PresenceAvailable24);
        IsClearCacheButtonEnabled = true;
    }

    [RelayCommand]
    private async Task Update()
    {
        IsUpdateButtonEnabled = false;
        await UpdateApp(true);
        IsUpdateButtonEnabled = true;
    }
}