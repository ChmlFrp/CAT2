using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ChmlFrp.SDK;
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
    [ObservableProperty] private NodeItem _selectedItem;

    [ObservableProperty] private Visibility _numberBoxVisibility = Visibility.Visible;
    [ObservableProperty] private string _remotePort;
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

    async partial void OnSelectedItemChanged(NodeItem value)
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

    public virtual async void LoadNodes(object sender, RoutedEventArgs e)
    { 
        await LoadNodesAsync();
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
            WritingLog(NodeDataContext.Count != 0 ? "节点数据加载成功" : "节点数据加载失败");
            return nodeData;
        }
}