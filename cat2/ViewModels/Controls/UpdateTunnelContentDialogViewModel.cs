using ChmlFrp.SDK;

namespace CAT2.ViewModels.Controls;

public class UpdateTunnelContentDialogViewModel(
    Classes.TunnelInfoClass tunnelInfo
) : AddTunnelContentDialogViewModel
{
    public override void LoadNodes(object sender, RoutedEventArgs e)
    {
        base.LoadNodes(sender, e);

        LocalIp = tunnelInfo.localip;
        LocalPort = tunnelInfo.nport.ToString();
        RemotePort = tunnelInfo.dorp;
        TunnelType = tunnelInfo.type;
    }
}