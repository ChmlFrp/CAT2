using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Threading;
using CAT2.Models;
using CSDK;

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
    public ObservableCollection<Items.StartedItem> AutoStartedItems { get; } = [];

    public async void Loaded(object sender, RoutedEventArgs e)
    {
        var deserialize = JsonNode.Parse(await File.ReadAllTextAsync(SettingsFilePath));
        IsAutoUpdatedEnabled = (bool)deserialize!["IsAutoUpdate"];

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

        List<Classes.TunnelInfoClass> tunnelsData;
        try
        {
            tunnelsData = await GetTunnelListAsync();
        }
        catch
        {
            return;
        }

        if (tunnelsData == null || tunnelsData.Count == 0) return;

        AutoStartedItems.Clear();
        foreach (var tunnelData in tunnelsData)
            AutoStartedItems.Add(new(tunnelData, false));

        foreach (var startedItem in AutoStartedItems.ToList())
            try
            {
                startedItem.IsStarted = (bool)deserialize!["StartedItems"]![startedItem.Name];
            }
            catch
            {
                // ignored
            }
    }

    [RelayCommand]
    private async Task WriteSettings()
    {
        var deserialize = JsonNode.Parse(await File.ReadAllTextAsync(SettingsFilePath));
        Dictionary<string, bool> items = [];
        foreach (var item in AutoStartedItems)
            if (item.IsStarted)
                items.Add(item.Name, true);
        deserialize!["StartedItems"] = JsonSerializer.SerializeToNode(items);
        await File.WriteAllTextAsync(SettingsFilePath, deserialize!.ToJsonString());
    }

    async partial void OnIsAutoUpdatedEnabledChanged(bool value)
    {
        var deserialize = JsonNode.Parse(await File.ReadAllTextAsync(SettingsFilePath));
        deserialize!["IsAutoUpdate"] = value;
        await File.WriteAllTextAsync(SettingsFilePath, deserialize!.ToJsonString());
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