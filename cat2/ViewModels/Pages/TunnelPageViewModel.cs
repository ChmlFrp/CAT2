using System.Collections.ObjectModel;
using System.Linq;
using CAT2.Views.Controls;
using static CAT2.Models.Items;

namespace CAT2.ViewModels;

public partial class TunnelPageViewModel : ObservableObject
{
    public async void Loaded(object sender, RoutedEventArgs e) => await Loaded();

    public ObservableCollection<TunnelItem> ListDataContext { get; } = [];

    [ObservableProperty] private bool _isLoadedEnabled;

    [RelayCommand] private async Task ShowDialog() => await new AddTunnelContentDialog(ContentDialogService.GetDialogHost(), this).ShowAsync();

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
            return;
        }

        if (tunnelsData.Count == 0)
        {
            WritingLog("没有隧道信息");
            return;
        }

        WritingLog($"加载到 {tunnelsData.Count} 个隧道信息");
        var tunnelsRunning = await IsTunnelRunningAsync(tunnelsData);

        ListDataContext.Clear();

        foreach (var item in tunnelsData.Select(tunnelData => new TunnelItem(this, tunnelData, tunnelsRunning[tunnelData.name])))
            ListDataContext.Add(item);

        IsLoadedEnabled = true;
    }
}