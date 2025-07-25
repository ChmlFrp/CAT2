using System.Collections.ObjectModel;
using CSDK;
using static CAT2.Models.Items;

namespace CAT2.ViewModels.Controls;

public partial class AddTunnelContentDialogViewModel : ObservableObject
{
    [ObservableProperty] private bool _isTunnelEnabled;
    [ObservableProperty] private string _localIp = "127.0.0.1";
    [ObservableProperty] private string _localPort;
    [ObservableProperty] private int _maximum;
    [ObservableProperty] private int _minimum;
    [ObservableProperty] private ObservableCollection<NodeItem> _nodeDataContext = [];
    [ObservableProperty] private NodeItem _nodeName;
    [ObservableProperty] private string _remotePort;
    [ObservableProperty] private string _tunnelType = "tcp";

    async partial void OnNodeNameChanged(NodeItem value)
    {
        var nodeInfo = await NodeActions.GetNodeInfoAsync(value.Name);
        if (nodeInfo == null) return;
        var sArray = nodeInfo.rport.Split('-');
        Minimum = int.Parse(sArray[0]);
        Maximum = int.Parse(sArray[1]);
    }

    partial void OnRemotePortChanged(string value)
    {
        IsTunnelEnabled = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(LocalPort);
    }

    partial void OnLocalPortChanged(string value)
    {
        IsTunnelEnabled = !string.IsNullOrEmpty(RemotePort) && !string.IsNullOrEmpty(value);
    }

    public async void LoadNodes(object sender, RoutedEventArgs e)
    {
        // 节点数据
        foreach (var nodeData in await NodeActions.GetNodesDataListAsync())
        {
            nodeData.udp = nodeData.udp == "true" ? "允许UDP" : "不允许UDP";
            nodeData.web = nodeData.web == "yes" ? "允许建站" : "不允许建站";
            nodeData.nodegroup = nodeData.nodegroup == "vip" ? "VIP节点" : "免费节点";
            NodeDataContext.Add(new NodeItem(nodeData));
        }

        WritingLog(NodeDataContext.Count != 0 ? "节点数据加载成功" : "节点数据加载失败");
    }
}