using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using CSDK;

namespace CAT2.ViewModels;

public partial class TunnelPageViewModel : ObservableObject
{
    private readonly Dictionary<string, Classes.TunnelInfoClass> _tunnelInfos = new();
    [ObservableProperty] private bool _isCreateTunnelFlyoutOpen;
    [ObservableProperty] private bool _isTunnelEnabled;
    [ObservableProperty] private ObservableCollection<TunnelItem> _listDataContext = [];
    [ObservableProperty] private string _localPort;

    [ObservableProperty] private int _maximum;
    [ObservableProperty] private int _minimum;
    [ObservableProperty] private ObservableCollection<NodeItem> _nodeDataContext = [];
    [ObservableProperty] private NodeItem _nodeName;
    [ObservableProperty] private ObservableCollection<TunnelItem> _offlinelist = [];
    [ObservableProperty] private string _remotePort;
    [ObservableProperty] private string _tunnelType;

    public TunnelPageViewModel()
    {
        LoadNodes();
        LoadTunnels(null, null);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        timer.Tick += LoadTunnels;
        timer.Start();
    }

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

    private async void LoadNodes()
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

    public async void LoadTunnels(object sender, EventArgs e)
    {
        var tunnelsData = await GetTunnelListAsync();
        if (tunnelsData == null)
        {
            WritingLog("隧道信息加载失败");
            ShowTip(
                "加载隧道信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
        else if (tunnelsData.Count == 0)
        {
            WritingLog("没有隧道信息");
            ShowTip(
                "没有隧道信息",
                "当前没有可用的隧道信息，请注册隧道。",
                ControlAppearance.Danger,
                SymbolRegular.Warning24);
            ListDataContext = [];
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
        }
    }

    [RelayCommand]
    private async Task CreateTunnel()
    {
        var msg = await CreateTunnelAsync(NodeName.Name, TunnelType, "127.0.0.1", LocalPort, RemotePort);

        WritingLog($"创建隧道返回：{msg}");

        if (string.IsNullOrEmpty(msg))
        {
            ShowTip(
                "隧道创建失败",
                "请检查网络状态，或查看API状态。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
        else if (msg.Contains("成功"))
        {
            ShowTip("隧道创建成功",
                $"{NodeName.Name}已添加至隧道列表。",
                ControlAppearance.Success,
                SymbolRegular.Checkmark24);
            RemotePort = string.Empty;
            LocalPort = string.Empty;
            LoadTunnels(null, null);
        }
        else
        {
            ShowTip("隧道创建失败",
                $"{msg}",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
    }

    [RelayCommand]
    private void ShowFlyout()
    {
        IsCreateTunnelFlyoutOpen = true;
    }
}

public partial class TunnelItem(
    TunnelPageViewModel parentViewModel,
    Classes.TunnelInfoClass tunnelData,
    bool istunnelstarted)
    : ObservableObject
{
    [ObservableProperty] private string _id = $"[隧道ID:{tunnelData.id}]";
    [ObservableProperty] private string _info = $"[节点名称:{tunnelData.node}]-[隧道类型:{tunnelData.type}]";
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private bool _isTunnelStarted = istunnelstarted;
    [ObservableProperty] private string _name = tunnelData.name;

    [ObservableProperty] private string _tooltip =
        $"[内网端口:{tunnelData.nport}]-[外网端口/连接域名:{tunnelData.dorp}]-[节点状态:{tunnelData.nodestate}]";


    partial void OnIsTunnelStartedChanged(bool value)
    {
        IsEnabled = false;
        if (value)
            StartTunnelAsync(
                tunnelData.name,
                () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip("隧道启动成功",
                            $"隧道 {tunnelData.name} 已成功启动，链接已复制到剪切板。",
                            ControlAppearance.Success,
                            SymbolRegular.Checkmark24);
                        IsEnabled = true;
                        Clipboard.SetDataObject($"{tunnelData.ip}:{tunnelData.dorp}");
                    });
                },
                () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip("隧道启动失败",
                            $"隧道 {tunnelData.name} 启动失败，具体请看日志。",
                            ControlAppearance.Danger,
                            SymbolRegular.TagError24);
                        IsTunnelStarted = false;
                        IsEnabled = true;
                    });
                },
                () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip(
                            "隧道启动数据获取失败",
                            "请检查网络状态，或查看API状态。",
                            ControlAppearance.Danger,
                            SymbolRegular.TagError24);
                        IsTunnelStarted = false;
                        IsEnabled = true;
                    });
                },
                () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip(
                            "FRPC 暂未安装",
                            "请等待一会，或重新启动。（软件会自动安装）",
                            ControlAppearance.Danger,
                            SymbolRegular.TagError24);
                        IsTunnelStarted = false;
                        IsEnabled = true;
                    });
                },
                () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip(
                            "隧道已在运行",
                            $"隧道 {tunnelData.name} 已在运行中。",
                            ControlAppearance.Danger,
                            SymbolRegular.Warning24);
                        IsEnabled = true;
                    });
                }
            );
        else
            StopTunnelAsync(
                tunnelData.name,
                () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip("隧道关闭成功",
                            $"隧道 {tunnelData.name} 已成功关闭。",
                            ControlAppearance.Success,
                            SymbolRegular.Checkmark24);
                        IsEnabled = true;
                    });
                },
                () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip("隧道关闭失败",
                            $"隧道 {tunnelData.name} 已退出。",
                            ControlAppearance.Danger,
                            SymbolRegular.TagError24);
                        IsEnabled = true;
                    });
                });
    }

    [RelayCommand]
    private async Task DeleteTunnel()
    {
        await StopTunnelAsync(tunnelData.name);
        await DeleteTunnelAsync(tunnelData.name);
        WritingLog($"删除隧道请求：{tunnelData.name}");

        ShowTip("隧道删除成功",
            $"隧道 {tunnelData.name} 已成功删除。",
            ControlAppearance.Success,
            SymbolRegular.Checkmark24);

        await Task.Delay(500);
        parentViewModel.LoadTunnels(null, null);
    }

    [RelayCommand]
    private void CopyTunnel()
    {
        try
        {
            Clipboard.SetDataObject($"{tunnelData.ip}:{tunnelData.dorp}", true);
        }
        catch
        {
            return;
        }

        WritingLog($"复制隧道链接：{tunnelData.ip}:{tunnelData.dorp}");
        ShowTip("链接已复制",
            $"隧道 {tunnelData.name} 的链接已复制到剪切板。",
            ControlAppearance.Success,
            SymbolRegular.Checkmark24);
    }
}

public partial class NodeItem(Classes.NodeDataClass nodeData) : ObservableObject
{
    [ObservableProperty] private string _content = $"{nodeData.name} ({nodeData.nodegroup})";
    [ObservableProperty] private string _name = nodeData.name;
    [ObservableProperty] private string _notes = $"{nodeData.notes} {nodeData.udp} {nodeData.web}";
}