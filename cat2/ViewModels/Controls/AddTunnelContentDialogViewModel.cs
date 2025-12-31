using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CAT2.ViewModels.Items;
using ChmlFrp.SDK;

namespace CAT2.ViewModels.Controls;

public partial class AddTunnelContentDialogViewModel : ObservableObject
{
    public AddTunnelContentDialogViewModel()
    {
        _ = LoadNodesAsync();
    }
    
    [ObservableProperty] private bool _isTunnelEnabled;
    [ObservableProperty] private string _localIp = "127.0.0.1";
    [ObservableProperty] private string _localPort;
    [ObservableProperty] private ObservableCollection<NodeViewModel> _nodeDataContext = [];

    [ObservableProperty] private Visibility _numberBoxVisibility = Visibility.Visible;
    [ObservableProperty] private string _remotePort;
    [ObservableProperty] private NodeViewModel _selectedItem;
    [ObservableProperty] private Visibility _textBoxVisibility = Visibility.Collapsed;
    [ObservableProperty] private string _tunnelType = "tcp";

    partial void OnTunnelTypeChanged(string value)
    {
        RemotePort = string.Empty;
        if (value is "HTTP" or "HTTPS")
        {
            NumberBoxVisibility = Visibility.Collapsed;
            TextBoxVisibility = Visibility.Visible;
        }
        else
        {
            NumberBoxVisibility = Visibility.Visible;
            TextBoxVisibility = Visibility.Collapsed;
        }
    }

    partial void OnRemotePortChanged(string value)
    {
        IsTunnelEnabled = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(LocalPort);
    }

    partial void OnLocalPortChanged(string value)
    {
        IsTunnelEnabled = !string.IsNullOrEmpty(RemotePort) && !string.IsNullOrEmpty(value);
    }

    protected async Task<List<Classes.NodeDataClass>> LoadNodesAsync()
    {
        var nodeData = await NodeActions.GetNodesDataListAsync();
        // 处理nodeData
        await Task.WhenAll(nodeData.Select(node =>
        {
            node.udp = node.udp == "true" ? "允许UDP" : "不允许UDP";
            node.web = node.web == "yes" ? "允许建站" : "不允许建站";
            node.nodegroup = node.nodegroup == "vip" ? "VIP节点" : "免费节点";
            NodeDataContext.Add(new(node));
            return Task.CompletedTask;
        }));

        return nodeData;
    }
}