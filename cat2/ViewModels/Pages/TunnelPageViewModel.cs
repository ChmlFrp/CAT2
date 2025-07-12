using System.Collections.ObjectModel;
using System.Windows.Threading;
using static ChmlFrp.SDK.API.Tunnel;
using static ChmlFrp.SDK.Services.Tunnel;

namespace CAT2.ViewModels;

public partial class TunnelPageViewModel : ObservableObject
{
    [ObservableProperty] private bool _isCreateTunnelFlyoutOpen;
    [ObservableProperty] private bool _isTunnelEnabled;

    // 隧道列表
    [ObservableProperty] private ObservableCollection<TunnelItem> _listDataContext;
    [ObservableProperty] private string _localPort;

    // 创建隧道
    [ObservableProperty] private ObservableCollection<NodeItem> _nodeDataContext;
    [ObservableProperty] private NodeItem _nodeName;
    [ObservableProperty] private ObservableCollection<TunnelItem> _offlinelist;
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
        NodeDataContext = [];
        foreach (var nodeData in await Node.GetNodesData())
        {
            nodeData.udp = nodeData.udp == "true" ? "允许UDP" : "不允许UDP";
            nodeData.web = nodeData.web == "yes" ? "允许建站" : "不允许建站";
            nodeData.nodegroup = nodeData.nodegroup == "vip" ? "VIP节点" : "免费节点";

            NodeDataContext.Add(new NodeItem
            {
                Name = nodeData.name,
                Content = $"{nodeData.name} ({nodeData.nodegroup})",
                Notes = $"{nodeData.notes} {nodeData.udp} {nodeData.web}"
            });
        }

        WritingLog(NodeDataContext.Count != 0 ? "节点数据加载成功" : "节点数据加载失败");
    }

    private async void LoadTunnels(object sender, EventArgs e)
    {
        ListDataContext = [];
        Offlinelist = [];
        var tunnelsData = await GetTunnelsData();
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
        }
        else
        {
            WritingLog($"加载到 {tunnelsData.Count} 个隧道信息");
            foreach (var tunnelData in tunnelsData)
            {
                var person = new TunnelItem(this, $"{tunnelData.ip}:{tunnelData.dorp}")
                {
                    Name = tunnelData.name,
                    Id = $"[隧道ID:{tunnelData.id}]",
                    IsTunnelStarted = await IsTunnelRunning(tunnelData.name),
                    Info = $"[节点名称:{tunnelData.node}]-[隧道类型:{tunnelData.type}]",
                    Tooltip = $"[内网端口:{tunnelData.nport}]-[外网端口/连接域名:{tunnelData.dorp}]-[节点状态:{tunnelData.nodestate}]"
                };

                ListDataContext.Add(person);
                if (tunnelData.nodestate != "online") Offlinelist.Add(person);
            }
        }
    }

    [RelayCommand]
    private async Task CreateTunnel()
    {
        var msg = await Tunnel.CreateTunnel(NodeName.Name, TunnelType, LocalPort, RemotePort);

        WritingLog($"创建隧道请求：{NodeName.Name} {TunnelType} {LocalPort} {RemotePort}");
        WritingLog($"创建隧道返回：{msg}");

        if (msg == null)
        {
            ShowTip("隧道创建失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
            return;
        }

        if (msg.Contains("成功"))
        {
            ShowTip("隧道创建成功",
                $"{msg}。",
                ControlAppearance.Success,
                SymbolRegular.Checkmark24);
            RemotePort = string.Empty;
            LocalPort = string.Empty;
            LoadTunnels(null, null);
            return;
        }

        ShowTip("隧道创建失败",
            msg,
            ControlAppearance.Danger,
            SymbolRegular.TagError24);
    }

    [RelayCommand]
    private void ShowFlyout()
    {
        IsCreateTunnelFlyoutOpen = true;
    }
}

public partial class TunnelItem(TunnelPageViewModel parentViewModel, string url) : ObservableObject
{
    [ObservableProperty] private string _id;
    [ObservableProperty] private string _info;
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private bool _isFlyoutOpen;
    [ObservableProperty] private bool _isTunnelStarted;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _tooltip;
    [ObservableProperty] private string _url = $"[连接地址:{url}]";

    [RelayCommand]
    private void Tunnel()
    {
        IsEnabled = false;
        if (IsTunnelStarted)
            StartTunnel(Name,
                () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip("隧道启动成功",
                            $"隧道 {Name} 已成功启动，链接已复制到剪切板。",
                            ControlAppearance.Success,
                            SymbolRegular.Checkmark24);
                        IsEnabled = true;
                        Clipboard.SetDataObject(url);
                    });
                },
                () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip("隧道启动失败",
                            $"隧道 {Name} 启动失败，具体请看日志。",
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
                            $"隧道 {Name} 已在运行中。",
                            ControlAppearance.Danger,
                            SymbolRegular.Warning24);
                        IsEnabled = true;
                    });
                }
            );
        else
            StopTunnel(Name,
                () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip("隧道关闭成功",
                            $"隧道 {Name} 已成功关闭。",
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
                            $"隧道 {Name} 已退出。",
                            ControlAppearance.Danger,
                            SymbolRegular.TagError24);
                        IsEnabled = true;
                    });
                });
    }


    [RelayCommand]
    private void DeleteTunnel()
    {
        StopTunnel(Name);
        ChmlFrp.SDK.API.Tunnel.DeleteTunnel(Name);
        WritingLog($"删除隧道请求：{Name}");

        ShowTip("隧道删除成功",
            $"隧道 {Name} 已成功删除。",
            ControlAppearance.Success,
            SymbolRegular.Checkmark24);

        parentViewModel.ListDataContext.Remove(this);
        parentViewModel.Offlinelist.Remove(this);
    }

    [RelayCommand]
    private void OpenFlyout()
    {
        IsFlyoutOpen = true;
    }

    [RelayCommand]
    private void CopyTunnel()
    {
        try
        {
            Clipboard.SetDataObject(url, true);
        }
        catch
        {
            return;
        }

        WritingLog($"复制隧道链接：{url}");
        ShowTip("链接已复制",
            $"隧道 {Name} 的链接已复制到剪切板。",
            ControlAppearance.Success,
            SymbolRegular.Checkmark24);
    }
}

public partial class NodeItem : ObservableObject
{
    [ObservableProperty] private string _content;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _notes;
}