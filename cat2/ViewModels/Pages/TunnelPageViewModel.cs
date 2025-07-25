using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CAT2.Controls;
using CSDK;
using static CAT2.Models.Items;

namespace CAT2.ViewModels;

public partial class TunnelPageViewModel : ObservableObject
{
    private readonly Dictionary<string, Classes.TunnelInfoClass> _tunnelInfos = new();

    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private ObservableCollection<TunnelItem> _listDataContext = [];
    [ObservableProperty] private ObservableCollection<TunnelItem> _offlinelist = [];

    public TunnelPageViewModel()
    {
        _ = LoadTunnels();
    }

    [RelayCommand]
    public async Task LoadTunnels()
    {
        IsEnabled = false;
        var tunnelsData = await GetTunnelListAsync();
        if (tunnelsData == null)
        {
            WritingLog("隧道信息加载失败");
            ShowSnackbar(
                "加载隧道信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
        else if (tunnelsData.Count == 0)
        {
            ListDataContext = [];
            WritingLog("没有隧道信息");
            ShowSnackbar(
                "没有隧道信息",
                "当前没有可用的隧道信息，请注册隧道。",
                ControlAppearance.Danger,
                SymbolRegular.Warning24);
        }
        else
        {
            WritingLog($"加载到 {tunnelsData.Count} 个隧道信息");
            var tunnelsRunning = await IsTunnelRunningAsync(tunnelsData);

            foreach (var tunnelData in tunnelsData)
            {
                if (!_tunnelInfos.TryAdd(tunnelData.name, tunnelData)) continue;
                var item = new TunnelItem(this, tunnelData, tunnelsRunning[tunnelData.name]);
                ListDataContext.Add(item);
                if (tunnelData.nodestate != "online") Offlinelist.Add(item);
            }

            foreach (var item in ListDataContext.ToList()
                         .Where(item => tunnelsData.All(tunnelData => tunnelData.name != item.Name)))
            {
                await StopTunnelAsync(item.Name);
                ListDataContext.Remove(item);
                Offlinelist.Remove(item);
            }

            ShowSnackbar(
                "加载隧道信息成功",
                "请查看您的隧道信息。",
                ControlAppearance.Success,
                SymbolRegular.Tag24);
        }

        IsEnabled = true;
    }


    [RelayCommand]
    private async Task ShowDialog()
    {
        _ = await new AddTunnelContentDialog(ContentDialogService.GetDialogHost()).ShowAsync();
    }
}