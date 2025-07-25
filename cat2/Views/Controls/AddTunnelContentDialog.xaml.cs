using System.Windows.Controls;
using CAT2.ViewModels.Controls;

namespace CAT2.Controls;

public partial class AddTunnelContentDialog
{
    private readonly AddTunnelContentDialogViewModel _viewModel = new();

    public AddTunnelContentDialog(ContentPresenter dialogHost) : base(dialogHost)
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += _viewModel.LoadNodes;
    }

    protected override async void OnButtonClick(ContentDialogButton button)
    {
        base.OnButtonClick(button);
        if (button != ContentDialogButton.Primary) return;

        var msg = await CreateTunnelAsync
        (
            _viewModel.NodeName.Name,
            _viewModel.TunnelType,
            _viewModel.LocalIp,
            _viewModel.LocalPort,
            _viewModel.RemotePort
        );

        WritingLog($"创建隧道返回：{msg}");

        if (string.IsNullOrEmpty(msg))
            ShowSnackbar(
                "隧道创建失败",
                "请检查网络状态，或查看API状态。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        else if (msg.Contains("成功"))
            ShowSnackbar("隧道创建成功",
                $"{_viewModel.NodeName.Name}已添加至隧道列表。",
                ControlAppearance.Success,
                SymbolRegular.Checkmark24);
        else
            ShowSnackbar("隧道创建失败",
                $"{msg}",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
    }
}