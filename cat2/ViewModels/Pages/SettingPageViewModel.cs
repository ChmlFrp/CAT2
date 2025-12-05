using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using CAT2.ViewModels.Items;
using CommunityToolkit.Mvvm.Input;
using static CAT2.Common.Constants;
using static ChmlFrp.SDK.TunnelActions;

namespace CAT2.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isClearedEnabled = true;

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
            var deserialize = JsonNode.Parse(await File.ReadAllTextAsync(SettingsFilePath));
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
            File.Delete(cacheFile);

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