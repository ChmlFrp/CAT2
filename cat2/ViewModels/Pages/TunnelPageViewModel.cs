using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Windows.Controls;
using CAT2.Views.Controls;
using static CAT2.Models.Items;

namespace CAT2.ViewModels;

public partial class TunnelPageViewModel : ObservableObject
{
    [ObservableProperty] private bool _isLoadedEnabled;
    [ObservableProperty] private SelectionMode _selectionMode;

    public ObservableCollection<TunnelItem> ListDataContext { get; } = [];

    public async void Loaded(object sender, RoutedEventArgs e)
    {
        await Loaded();
    }

    [RelayCommand]
    private async Task ShowDialog()
    {
        await new AddTunnelContentDialog(ContentDialogService.GetDialogHost(), this).ShowAsync();
    }

    [RelayCommand]
    private async Task Loaded()
    {
        IsLoadedEnabled = false;
        var tunnelsData = await GetTunnelListAsync();

        if (tunnelsData == null)
        {
            WritingLog("隧道信息加载失败");
            ShowSnackbar(
                "加载隧道信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
            IsLoadedEnabled = true;
            return;
        }

        if (tunnelsData.Count == 0)
        {
            WritingLog("没有隧道信息");
            ListDataContext.Clear();
            IsLoadedEnabled = true;
            return;
        }

        WritingLog($"加载到 {tunnelsData.Count} 个隧道信息");

        var runningTunnels = await IsTunnelRunningAsync(tunnelsData);
        var settings = JsonNode.Parse(await File.ReadAllTextAsync(SettingsFilePath))?["StartedItems"];
        
        var existingItems = ListDataContext.ToDictionary(item => item.Id);

        await Task.WhenAll(tunnelsData.Select(tunnel =>
        {
            var tunnelId = $"[隧道ID:{tunnel.id}]";
            var isRunning = runningTunnels[tunnel.id];

            if (existingItems.TryGetValue(tunnelId, out var existingItem))
            {
                existingItem.IsStarted = isRunning;
                if (isRunning ||
                    settings?[$"{tunnel.name}({tunnel.type.ToUpperInvariant()})"] is not JsonValue value ||
                    !value.TryGetValue<bool>(out var isStarted) ||
                    !isStarted) return Task.CompletedTask;
                existingItem.IsStarted = true;
                existingItem.TunnelClick();
            }
            else
            {
                var newItem = new TunnelItem(this, tunnel, isRunning);
                if (!isRunning && 
                    settings?[$"{tunnel.name}({tunnel.type.ToUpperInvariant()})"] is JsonValue value &&
                    value.TryGetValue<bool>(out var isStarted) && 
                    isStarted)
                {
                    newItem.IsStarted = true;
                    newItem.TunnelClick();
                }
        
                ListDataContext.Add(newItem);
            }

            return Task.CompletedTask;
        }));
     
        var validIds = new HashSet<string>(tunnelsData.Select(t => $"[隧道ID:{t.id}]"));
        foreach (var item in ListDataContext
                     .Where(item => !validIds.Contains(item.Id))
                     .ToList())
            ListDataContext.Remove(item);
        
        IsLoadedEnabled = true;
    }
}