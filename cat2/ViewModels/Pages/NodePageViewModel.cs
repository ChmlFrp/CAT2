using System.Collections.ObjectModel;
using System.Linq;
using ChmlFrp.SDK;
using static CAT2.Models.Items;

namespace CAT2.ViewModels;

public partial class NodePageViewModel : ObservableObject
{
    [ObservableProperty] private bool _isLoadedEnabled;

    public ObservableCollection<NodeInfoItem> ListDataContext { get; } = [];

    public async void Loaded(object sender, RoutedEventArgs e)
    {
        await Loaded();
    }

    [RelayCommand]
    private async Task Loaded()
    {
        IsLoadedEnabled = false;
        
        var nodesData = await NodeActions.GetNodesDataListAsync();
        ListDataContext.Clear();
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

        IsLoadedEnabled = true;
    }
}