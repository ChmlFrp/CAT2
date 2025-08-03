using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Threading;
using CAT2.Models;

namespace CAT2.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
    [ObservableProperty] private string _assemblyName = Constants.AssemblyName;

    [ObservableProperty] private string _context = "正在检测...";
    [ObservableProperty] private string _copyright = Constants.Copyright;
    [ObservableProperty] private string _fileVersion = $"文件版本：{Constants.FileVersion}";

    [ObservableProperty] private bool _isAutoUpdatedEnabled;

    [ObservableProperty] private bool _isClearedEnabled = true;

    [ObservableProperty] private bool _isUpdatedEnabled = true;
    [ObservableProperty] private string _version = Constants.Version;

    public async void Loaded(object sender, RoutedEventArgs e)
    {
        var data = await File.ReadAllTextAsync(SettingsFilePath);
        var deserialize = JsonSerializer.Deserialize<Dictionary<string, bool>>(data);
        IsAutoUpdatedEnabled = deserialize["IsAutoUpdate"];

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

    async partial void OnIsAutoUpdatedEnabledChanged(bool value)
    {
        if (value)
            await File.WriteAllTextAsync(SettingsFilePath,
                JsonSerializer.Serialize(new Dictionary<string, bool> { { "IsAutoUpdate", true } }));
        else
            await File.WriteAllTextAsync(SettingsFilePath,
                JsonSerializer.Serialize(new Dictionary<string, bool> { { "IsAutoUpdate", false } }));
    }

    [RelayCommand]
    private void Cleared()
    {
        IsClearedEnabled = false;
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
        IsClearedEnabled = true;
    }

    [RelayCommand]
    private async Task Updated()
    {
        IsUpdatedEnabled = false;
        await UpdateApp(true);
        IsUpdatedEnabled = true;
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
}