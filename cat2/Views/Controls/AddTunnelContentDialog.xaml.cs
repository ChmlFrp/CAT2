using System.Windows.Controls;
using CAT2.ViewModels.Controls;

namespace CAT2.Views.Controls;

public partial class AddTunnelContentDialog
{
    private readonly TunnelPageViewModel _parentViewModel;
    private readonly AddTunnelContentDialogViewModel _viewModel;

    public AddTunnelContentDialog
    (
        ContentPresenter dialogHost,
        TunnelPageViewModel parentViewModel = null
    ) : base(dialogHost)
    {
        _parentViewModel = parentViewModel;
        InitializeComponent();
        _viewModel = (AddTunnelContentDialogViewModel)DataContext;
        Loaded += _viewModel.LoadNodes;
    }

    protected override async void OnButtonClick(ContentDialogButton button)
    {
        base.OnButtonClick(button);
        if (button != ContentDialogButton.Primary) return;

        var msg = await CreateTunnelAsync
        (
            _viewModel.SelectedItem.Name,
            _viewModel.TunnelType.ToLowerInvariant(),
            _viewModel.LocalIp,
            _viewModel.LocalPort,
            _viewModel.RemotePort
        );

        WritingLog($"创建隧道返回：{msg}");

        if (string.IsNullOrEmpty(msg))
            ShowSnackBar(
                "隧道创建失败",
                "请检查网络状态，或查看API状态。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        else if (msg.Contains("成功"))
            ShowSnackBar("隧道创建成功",
                "已添加至隧道列表。",
                ControlAppearance.Success,
                SymbolRegular.Checkmark24);
        else
            ShowSnackBar("隧道创建失败",
                $"{msg}",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);

        await Task.Delay(500);
        _parentViewModel?.Loaded(null, null);
    }
}