using System.Collections.ObjectModel;
using CAT2.Views.Controls;
using static CAT2.Models.Items;

namespace CAT2.ViewModels;

public partial class TunnelPageViewModel : ObservableObject
{
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private ObservableCollection<TunnelItem> _listDataContext = [];
    [ObservableProperty] private ObservableCollection<TunnelItem> _offlinelist = [];

    public TunnelPageViewModel()
    {
        _ = Loading();
    }

    [RelayCommand]
    private async Task Loading()
    {
        await LoadTunnels(true);
    }

    public async Task LoadTunnels(bool isShow)
    {
        IsEnabled = false;
        var tunnelsData = await GetTunnelListAsync();
        ListDataContext = [];
        Offlinelist = [];
        if (tunnelsData == null)
        {
            WritingLog("隧道信息加载失败");
            if (isShow)
                ShowSnackbar(
                    "加载隧道信息失败",
                    "请检查网络连接或稍后重试。",
                    ControlAppearance.Danger,
                    SymbolRegular.TagError24);
        }
        else if (tunnelsData.Count == 0)
        {
            WritingLog("没有隧道信息");
            if (isShow)
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
                var item = new TunnelItem(this, tunnelData, tunnelsRunning[tunnelData.name]);
                ListDataContext.Add(item);
                if (tunnelData.nodestate != "online") Offlinelist.Add(item);
            }

            if (isShow)
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
        await new AddTunnelContentDialog(ContentDialogService.GetDialogHost(), this).ShowAsync();
    }
}