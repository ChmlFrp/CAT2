using CAT2.Views.Controls;
using ChmlFrp.SDK;
using CommunityToolkit.Mvvm.Input;

namespace CAT2.ViewModels.Items;

public partial class NodeInfoViewModel(
    Classes.NodeInfoClass nodeInfo
) : ObservableObject
{
    [ObservableProperty]
    private string _id = $"#{nodeInfo.id}";

    [ObservableProperty]
    private float _load15 = nodeInfo.load15 * 100;

    [ObservableProperty]
    private string _name = $"{nodeInfo.name}({(nodeInfo.nodegroup == "vip" ? "VIP" : "普通")},{(nodeInfo.state == "online" ? "在线" : "离线")})";

    [ObservableProperty]
    private string _notes = $"[IP地址:{nodeInfo.ip}]\n[CPU:{nodeInfo.cpu_info}]\n[介绍:{nodeInfo.notes}]";

    [ObservableProperty]
    private string _totalTrafficIn = $"[下载流量:{nodeInfo.total_traffic_in / 1024 / 1024 / 1024:0.0}GB]";

    [ObservableProperty]
    private string _totalTrafficOut = $"[上传流量： {nodeInfo.total_traffic_out / 1024 / 1024 / 1024:0.0}GB]";

    [RelayCommand]
    private async Task ShowAddTunnelDialogAsync()
    {
        await new AddTunnelContentDialog(ContentDialogService.GetDialogHost()).ShowAsync();
    }
}