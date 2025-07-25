﻿using CAT2.ViewModels;
using CSDK;

namespace CAT2.Models;

public static partial class Items
{
    public partial class TunnelItem(
        TunnelPageViewModel parentViewModel,
        Classes.TunnelInfoClass tunnelData,
        bool isTunnelStarted
    ) : ObservableObject
    {
        [ObservableProperty] private string _id = $"[隧道ID:{tunnelData.id}]";
        [ObservableProperty] private string _info = $"[节点名称:{tunnelData.node}]-[隧道类型:{tunnelData.type}]";
        [ObservableProperty] private bool _isEnabled = true;
        [ObservableProperty] private bool _isTunnelStarted = isTunnelStarted;
        [ObservableProperty] private string _name = tunnelData.name;

        [ObservableProperty] private string _toolTip =
            $"[内网端口:{tunnelData.nport}]-[外网端口/连接域名:{tunnelData.dorp}]-[节点状态:{tunnelData.nodestate}]";


        async partial void OnIsTunnelStartedChanged(bool value)
        {
            IsEnabled = false;
            if (value)
                await StartTunnelAsync(
                    tunnelData.name,
                    () =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowSnackbar("隧道启动成功",
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
                            ShowSnackbar("隧道启动失败",
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
                            ShowSnackbar(
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
                            ShowSnackbar(
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
                            ShowSnackbar(
                                "隧道已在运行",
                                $"隧道 {tunnelData.name} 已在运行中。",
                                ControlAppearance.Danger,
                                SymbolRegular.Warning24);
                            IsEnabled = true;
                        });
                    }
                );
            else
                await StopTunnelAsync(
                    tunnelData.name,
                    () =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowSnackbar("隧道关闭成功",
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
                            ShowSnackbar("隧道关闭失败",
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
            if (await ShowConfirm(
                    "你确定要删除吗?",
                    "在删除前，问一个问题：你到底有多少力量?\no(￣┰￣*)ゞ",
                    "下定决心",
                    "就此罢手") != ContentDialogResult.Primary) return;

            await StopTunnelAsync(tunnelData.name);
            await DeleteTunnelAsync(tunnelData.name);
            WritingLog($"删除隧道请求：{tunnelData.name}");

            ShowSnackbar("隧道删除成功",
                $"隧道 {tunnelData.name} 已成功删除。",
                ControlAppearance.Success,
                SymbolRegular.Checkmark24);

            await Task.Delay(500);
            await parentViewModel.LoadTunnels();
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
            ShowSnackbar("链接已复制",
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
}