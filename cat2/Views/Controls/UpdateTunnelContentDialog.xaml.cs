using System.Windows.Controls;
using CAT2.ViewModels.Controls;
using ChmlFrp.SDK;

namespace CAT2.Views.Controls;

public partial class UpdateTunnelContentDialog
{
    private readonly TunnelPageViewModel _parentViewModel;
    private readonly Classes.TunnelInfoClass _tunnelInfo;
    private readonly UpdateTunnelContentDialogViewModel _viewModel;

    public UpdateTunnelContentDialog
    (
        ContentPresenter dialogHost,
        Classes.TunnelInfoClass tunnelInfo,
        TunnelPageViewModel parentViewModel
    ) : base(dialogHost)
    {
        _tunnelInfo = tunnelInfo;
        _parentViewModel = parentViewModel;
        InitializeComponent();

        _viewModel = new UpdateTunnelContentDialogViewModel(_tunnelInfo);
        DataContext = _viewModel;
        Loaded += _viewModel.LoadNodes;
    }

    protected override async void OnButtonClick(ContentDialogButton button)
    {
        base.OnButtonClick(button);
        if (button != ContentDialogButton.Primary) return;

        var msg = await UpdateTunnelAsync
        (
            _tunnelInfo,
            _viewModel.NodeName.Name,
            _viewModel.TunnelType.ToLowerInvariant(),
            _viewModel.LocalIp,
            _viewModel.LocalPort,
            _viewModel.RemotePort
        );

        WritingLog($"更新隧道返回：{msg}");

        if (string.IsNullOrEmpty(msg))
            ShowSnackBar(
                "隧道更新失败",
                "请检查网络状态，或查看API状态。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        else if (msg.Contains("成功"))
            ShowSnackBar("隧道更新成功",
                $"{_tunnelInfo.name}已更新至隧道列表。",
                ControlAppearance.Success,
                SymbolRegular.Checkmark24);
        else
            ShowSnackBar("隧道创建失败",
                $"{msg}",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);

        await Task.Delay(500);
        _parentViewModel.Loaded(null, null);
    }
}