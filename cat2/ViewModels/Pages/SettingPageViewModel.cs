using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using CAT2.ViewModels.Items;
using CommunityToolkit.Mvvm.Input;
using static ChmlFrp.SDK.TunnelActions;

namespace CAT2.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
    public SettingPageViewModel()
    {
        _ = Loaded();
    }

    [ObservableProperty]
    private Visibility _labelVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility _listVisibility = Visibility.Collapsed;
    
    public ObservableCollection<TunnelStartedViewModel> AutoStartedItems { get; } = [];

    public async Task Loaded()
    {
        var tunnelsData = await GetTunnelListAsync();
        if (tunnelsData.Count == 0)
        {
            LabelVisibility = Visibility.Visible;
            ListVisibility = Visibility.Collapsed;
        }
        else
        {
            LabelVisibility = Visibility.Collapsed;
            ListVisibility = Visibility.Visible;

            AutoStartedItems.Clear();
            var deserialize = JsonNode.Parse(await File.ReadAllTextAsync(App.SettingsFilePath));
            foreach (var tunnelData in tunnelsData)
                if (deserialize?["StartedItems"]?[$"{tunnelData.name}({tunnelData.type.ToUpperInvariant()})"] is
                        JsonValue
                        startedValue &&
                    startedValue.TryGetValue<bool>(out var isStarted))
                    AutoStartedItems.Add(new(tunnelData, isStarted));
                else
                    AutoStartedItems.Add(new(tunnelData, false));
        }
    }

    [RelayCommand]
    private async Task WriteSettings()
    {
        var deserialize = JsonNode.Parse(await File.ReadAllTextAsync(App.SettingsFilePath));
        Dictionary<string, bool> items = [];
        foreach (var item in AutoStartedItems)
            if (item.IsStarted)
                items.Add(item.Name, true);
        deserialize!["StartedItems"] = JsonSerializer.SerializeToNode(items);
        await File.WriteAllTextAsync(App.SettingsFilePath, deserialize!.ToJsonString());
    }

    [RelayCommand]
    private void Cleared()
    {
        foreach (var cacheFile in Directory.GetFiles(DataPath, "*.log"))
            File.Delete(cacheFile);

        App.ShowSnackBar(
            "缓存已清理",
            "所有缓存文件已被删除。",
            ControlAppearance.Success,
            SymbolRegular.PresenceAvailable24);
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