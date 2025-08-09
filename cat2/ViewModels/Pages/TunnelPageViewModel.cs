using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Nodes;
using CAT2.Views.Controls;
using static CAT2.Models.Items;

namespace CAT2.ViewModels;

public partial class TunnelPageViewModel : ObservableObject
{
    [ObservableProperty] private bool _isLoadedEnabled;

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
        var tunnelsRunning = IsTunnelRunning(tunnelsData);
        var deserialize = JsonNode.Parse(await File.ReadAllTextAsync(SettingsFilePath));
        ListDataContext.Clear();

        foreach (var tunnelData in tunnelsData)
        {
            var isRunning = tunnelsRunning[tunnelData.id];
            var item = new TunnelItem(this, tunnelData, isRunning);
            ListDataContext.Add(item);
            if (isRunning) continue;

            if (deserialize?["StartedItems"]?[$"{tunnelData.name}({tunnelData.type.ToUpperInvariant()})"] is not
                    JsonValue
                    startedValue ||
                !startedValue.TryGetValue<bool>(out var isStarted)) continue;
            item.IsStarted = isStarted;
            if (isStarted)
                item.TunnelClick();
        }

        IsLoadedEnabled = true;
    }
}