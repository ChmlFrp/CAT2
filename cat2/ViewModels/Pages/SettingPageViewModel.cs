using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using CAT2.Models;

namespace CAT2.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
    [ObservableProperty] private string _assemblyName = Constants.AssemblyName;
    [ObservableProperty] private string _copyright = Constants.Copyright;
    [ObservableProperty] private string _fileVersion = $"文件版本：{Constants.FileVersion}";
    [ObservableProperty] private bool _isClearedEnabled = true;
    [ObservableProperty] private Visibility _labelVisibility = Visibility.Visible;
    [ObservableProperty] private Visibility _listVisibility = Visibility.Collapsed;
    [ObservableProperty] private string _version = Constants.Version;
    public ObservableCollection<Items.StartedItem> AutoStartedItems { get; } = [];

    public async void Loaded(object sender, RoutedEventArgs e)
    {
        var tunnelsData = await GetTunnelListAsync();
        if (tunnelsData.Count == 0)
        {
            LabelVisibility = Visibility.Visible;
            ListVisibility = Visibility.Collapsed;
            return;
        }

        LabelVisibility = Visibility.Collapsed;
        ListVisibility = Visibility.Visible;

        AutoStartedItems.Clear();
        var deserialize = JsonNode.Parse(await File.ReadAllTextAsync(SettingsFilePath));
        foreach (var tunnelData in tunnelsData)
            if (deserialize?["StartedItems"]?[$"{tunnelData.name}({tunnelData.type.ToUpperInvariant()})"] is JsonValue
                    startedValue &&
                startedValue.TryGetValue<bool>(out var isStarted))
                AutoStartedItems.Add(new(tunnelData, isStarted));
            else
                AutoStartedItems.Add(new(tunnelData, false));
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

        ShowSnackBar(
            "缓存已清理",
            "所有缓存文件已被删除。",
            ControlAppearance.Success,
            SymbolRegular.PresenceAvailable24);
        IsClearedEnabled = true;
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