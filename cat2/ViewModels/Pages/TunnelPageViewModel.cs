using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using CAT2.ViewModels.Items;
using CAT2.Views.Controls;
using CommunityToolkit.Mvvm.Input;
using static CAT2.Common.Constants;
using static ChmlFrp.SDK.TunnelActions;

namespace CAT2.ViewModels;

public partial class TunnelPageViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoadedEnabled;

    public ObservableCollection<TunnelViewModel> ListDataContext { get; } = [];

    [RelayCommand]
    private async Task ShowDialog()
    {
        await new AddTunnelContentDialog(ContentDialogService.GetDialogHost(), this).ShowAsync();
    }

    [RelayCommand]
    private async Task ClearFrpc()
    {
        var processes = Process.GetProcessesByName("frpc");
        processes.ToList().ForEach(x => x.Kill());
        await Loaded();
    }

    [RelayCommand]
    public async Task Loaded()
    {
        IsLoadedEnabled = false;
        var tunnelsData = await GetTunnelListAsync();

        if (tunnelsData.Count == 0)
        {
            ListDataContext.Clear();
            IsLoadedEnabled = true;
            return;
        }

        WritingLog($"加载到 {tunnelsData.Count} 个隧道信息");

        var runningTunnels = await IsTunnelRunningAsync(tunnelsData);
        var settings = JsonNode.Parse(await File.ReadAllTextAsync(SettingsFilePath))?["StartedItems"];

        ListDataContext.Clear();
        await Task.WhenAll(tunnelsData.Select(async tunnel =>
        {
            var newItem = new TunnelViewModel(this, tunnel, runningTunnels[tunnel.id]);
            ListDataContext.Add(newItem);
            if (settings?[$"{tunnel.name}({tunnel.type.ToUpperInvariant()})"] is not JsonValue value ||
                !value.TryGetValue<bool>(out var isStarted) ||
                !isStarted || newItem.IsStarted) return Task.CompletedTask;
            newItem.IsStarted = true;
            await newItem.OnTunnelClick();
            return Task.CompletedTask;
        }));

        IsLoadedEnabled = true;
    }
}