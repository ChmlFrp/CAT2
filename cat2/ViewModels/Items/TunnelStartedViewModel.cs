using ChmlFrp.SDK;

namespace CAT2.ViewModels.Items;

public partial class TunnelStartedViewModel(
    Classes.TunnelInfoClass tunnelInfo,
    bool isStarted
) : ObservableObject
{
    [ObservableProperty] 
    private bool _isStarted = isStarted;
    
    [ObservableProperty] 
    private string _name = $"{tunnelInfo.name}({tunnelInfo.type.ToUpperInvariant()})";
}