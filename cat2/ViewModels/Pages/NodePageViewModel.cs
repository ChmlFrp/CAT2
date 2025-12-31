using System.Collections.ObjectModel;
using System.Linq;
using CAT2.ViewModels.Items;
using ChmlFrp.SDK;
using CommunityToolkit.Mvvm.Input;

namespace CAT2.ViewModels;

public partial class NodePageViewModel : ObservableObject
{
    public NodePageViewModel()
    {
        // _ = Loaded();
    }
    
    public ObservableCollection<NodeInfoViewModel> ListDataContext { get; } = [];

    [RelayCommand]
    public async Task Loaded()
    {
        var nodesData = await NodeActions.GetNodesDataListAsync();
        await Task.WhenAll(nodesData.Select(async nodeData =>
        {
            try
            {
                var nodeInfo = await NodeActions.GetNodeInfoAsync(nodeData.name);
                ListDataContext.Add(new(nodeInfo));
            }
            catch
            {
                // ignored
            }
        }));
    }
}