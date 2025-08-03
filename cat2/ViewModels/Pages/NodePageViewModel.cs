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

        ListDataContext.Clear();
        foreach (var nodeData in await NodeActions.GetNodesDataListAsync())
            ListDataContext.Add
            (
                new NodeInfoItem
                (
                    await NodeActions.GetNodeInfoAsync
                    (
                        nodeData.name
                    )
                )
            );

        IsLoadedEnabled = true;
    }
}