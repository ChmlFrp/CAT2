using ChmlFrp.SDK;

namespace CAT2.ViewModels.Controls;

public class UpdateTunnelContentDialogViewModel(
    Classes.TunnelInfoClass tunnelInfo
) : AddTunnelContentDialogViewModel
{
    public async Task LoadNodes()
    {
        LocalIp = tunnelInfo.localip;
        LocalPort = tunnelInfo.nport.ToString();
        TunnelType = tunnelInfo.type.ToUpperInvariant();
        RemotePort = tunnelInfo.dorp;
        var nodeData = await LoadNodesAsync();
        if (nodeData.Count == 0) return;
        SelectedItem = NodeDataContext[nodeData.FindIndex(node => node.name == tunnelInfo.node)];
    }
}