using System.Collections.Generic;
using System.Collections.ObjectModel;
using CSDK;
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

        List<Classes.NodeInfoClass> nodeInfoList = [];
        foreach (var nodeData in await NodeActions.GetNodesDataListAsync())
            nodeInfoList.Add(await NodeActions.GetNodeInfoAsync
            (
                nodeData.name
            ));
        ListDataContext.Clear();

        foreach (var nodeInfo in nodeInfoList)
            ListDataContext.Add(new(nodeInfo));

        IsLoadedEnabled = true;
    }
}