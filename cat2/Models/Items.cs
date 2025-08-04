using CAT2.ViewModels;
using CAT2.Views.Controls;
using static CSDK.Classes;

namespace CAT2.Models;

public static partial class Items
{
    public partial class TunnelItem(
        TunnelPageViewModel parentViewModel,
        TunnelInfoClass tunnelInfo,
        bool isStarted
    ) : ObservableObject
    {
        [ObservableProperty] private string _id = $"[隧道ID:{tunnelInfo.id}]";
        [ObservableProperty] private string _info = $"[节点名称:{tunnelInfo.node}]-[隧道类型:{tunnelInfo.type}]";
        [ObservableProperty] private bool _isStarted = isStarted;

        [ObservableProperty] private bool _isStartedEnabled = true;
        [ObservableProperty] private string _name = tunnelInfo.name;

        [ObservableProperty]
        private string _toolTip =
            $"[内网端口:{tunnelInfo.nport}]-[外网端口/连接域名:{tunnelInfo.dorp}]-[节点状态:{tunnelInfo.nodestate}]";

        async partial void OnIsStartedChanged(bool value)
        {
            IsStartedEnabled = false;
            if (value)
                await StartTunnelAsync(
                    tunnelInfo.name,
                    () =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowSnackbar("隧道启动成功",
                                $"隧道 {tunnelInfo.name} 已成功启动，链接已复制到剪切板。",
                                ControlAppearance.Success,
                                SymbolRegular.Checkmark24);
                            IsStartedEnabled = true;
                            Clipboard.SetDataObject($"{tunnelInfo.ip}:{tunnelInfo.dorp}");
                        });
                    },
                    () =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowSnackbar("隧道启动失败",
                                $"隧道 {tunnelInfo.name} 启动失败，具体请看日志。",
                                ControlAppearance.Danger,
                                SymbolRegular.TagError24);
                            IsStarted = false;
                            IsStartedEnabled = true;
                        });
                    },
                    () =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowSnackbar(
                                "隧道启动数据获取失败",
                                "请检查网络状态，或查看API状态。",
                                ControlAppearance.Danger,
                                SymbolRegular.TagError24);
                            IsStarted = false;
                            IsStartedEnabled = true;
                        });
                    },
                    () =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowSnackbar(
                                "FRPC 暂未安装",
                                "请等待一会，或重新启动。（软件会自动安装）",
                                ControlAppearance.Danger,
                                SymbolRegular.TagError24);
                            IsStarted = false;
                            IsStartedEnabled = true;
                        });
                    },
                    () =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowSnackbar(
                                "隧道已在运行",
                                $"隧道 {tunnelInfo.name} 已在运行中。",
                                ControlAppearance.Danger,
                                SymbolRegular.Warning24);
                            IsStartedEnabled = true;
                        });
                    }
                );
            else
                await StopTunnelAsync(
                    tunnelInfo.name,
                    () =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowSnackbar("隧道关闭成功",
                                $"隧道 {tunnelInfo.name} 已成功关闭。",
                                ControlAppearance.Success,
                                SymbolRegular.Checkmark24);
                            IsStartedEnabled = true;
                        });
                    },
                    () =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowSnackbar("隧道关闭失败",
                                $"隧道 {tunnelInfo.name} 已退出。",
                                ControlAppearance.Danger,
                                SymbolRegular.TagError24);
                            IsStartedEnabled = true;
                        });
                    });
        }

        [RelayCommand]
        private async Task DeleteTunnel()
        {
            if (await ShowConfirm(
                    "你确定要删除吗?",
                    "在删除前，问一个问题：你到底有多少力量?\no(￣┰￣*)ゞ",
                    "下定决心",
                    "就此罢手") != ContentDialogResult.Primary) return;

            await DeleteTunnelAsync(tunnelInfo.name);
            WritingLog($"删除隧道请求：{tunnelInfo.name}");

            ShowSnackbar("隧道删除成功",
                $"隧道 {tunnelInfo.name} 已成功删除。",
                ControlAppearance.Success,
                SymbolRegular.Checkmark24);

            await Task.Delay(500);
            parentViewModel.Loaded(null, null);
        }

        [RelayCommand]
        private void CopyTunnel()
        {
            try
            {
                Clipboard.SetDataObject($"{tunnelInfo.ip}:{tunnelInfo.dorp}", true);
            }
            catch
            {
                return;
            }

            WritingLog($"复制隧道链接：{tunnelInfo.ip}:{tunnelInfo.dorp}");
            ShowSnackbar("链接已复制",
                $"隧道 {tunnelInfo.name} 的链接已复制到剪切板。",
                ControlAppearance.Success,
                SymbolRegular.Checkmark24);
        }

        [RelayCommand]
        private async Task UpdateTunnel()
        {
            await new UpdateTunnelContentDialog(
                ContentDialogService.GetDialogHost(),
                tunnelInfo,
                parentViewModel
            ).ShowAsync();
        }
    }

    public partial class NodeItem(
        NodeDataClass nodeData
    ) : ObservableObject
    {
        [ObservableProperty]
        private string _content = $"{nodeData.name} ({nodeData.nodegroup}，{nodeData.udp}，{nodeData.web})";

        [ObservableProperty] private string _name = nodeData.name;

        [ObservableProperty] private string _notes = $"{nodeData.notes}";
    }

    public partial class NodeInfoItem(
        NodeInfoClass nodeInfo
    ) : ObservableObject
    {
        [ObservableProperty] private string _id = $"#{nodeInfo.id}";
        [ObservableProperty] private float _load15 = nodeInfo.load15 * 100;

        [ObservableProperty]
        private string _name =
            $"{nodeInfo.name}({(nodeInfo.nodegroup == "vip" ? "VIP" : "普通")},{(nodeInfo.state == "online" ? "在线" : "离线")})";

        [ObservableProperty]
        private string _notes = $"IP地址： {nodeInfo.ip}\nCPU： {nodeInfo.cpu_info}\n介绍: {nodeInfo.notes}";

        [ObservableProperty]
        private string _totalTrafficIn = $"下载流量： {nodeInfo.total_traffic_in / 1024 / 1024 / 1024:0.0}GB";

        [ObservableProperty]
        private string _totalTrafficOut = $"上传流量： {nodeInfo.total_traffic_out / 1024 / 1024 / 1024:0.0}GB";
    }

    public partial class StartedItem(
        TunnelInfoClass tunnelInfo,
        bool isStarted
    ) : ObservableObject
    {
        [ObservableProperty] private bool _isStarted = isStarted;

        [ObservableProperty] private string _name = $"{tunnelInfo.name}({tunnelInfo.type.ToUpper()})";
    }
}