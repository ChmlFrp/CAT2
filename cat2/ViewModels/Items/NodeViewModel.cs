using ChmlFrp.SDK;

namespace CAT2.ViewModels.Items;

public partial class NodeViewModel(
    Classes.NodeDataClass nodeData
) : ObservableObject
{
    [ObservableProperty]
    private string _content = $"{nodeData.name} ({nodeData.nodegroup}，{nodeData.udp}，{nodeData.web})";

    [ObservableProperty] 
    private string _name = nodeData.name;

    [ObservableProperty] 
    private string _notes = $"{nodeData.notes}";
}