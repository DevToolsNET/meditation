using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meditation.AttachProcessService;
using Meditation.AttachProcessService.Models;
using Meditation.UI.Utilities;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.UI.ViewModels
{
    public partial class ProcessListViewModel : ViewModelBase
    {
        private readonly IAttachableProcessesAggregator _processListAggregator;

        [ObservableProperty] private FilterableCollectionView<ProcessInfo> _processList;
        [ObservableProperty] private ProcessInfo? _selectedProcess;
        [ObservableProperty] private string? _nameFilter;

        public ProcessListViewModel(IAttachableProcessesAggregator processListAggregator)
        {
            _processListAggregator = processListAggregator;
            ProcessList = new FilterableCollectionView<ProcessInfo>(GetAttachableProcessesAsync(CancellationToken.None));
        }

        [RelayCommand]
        public void FilterProcessList()
        {
            var filteredProcessList = new FilterableCollectionView<ProcessInfo>(GetAttachableProcessesAsync(CancellationToken.None));
            filteredProcessList.ApplyFilter(p => NameFilter == null || p.Name.Contains(NameFilter));
            ProcessList = filteredProcessList;
        }

        [RelayCommand]
        public void RefreshProcessList()
        {
            _processListAggregator.Refresh();
            ProcessList = new FilterableCollectionView<ProcessInfo>(GetAttachableProcessesAsync(CancellationToken.None));
        }

        private async Task<ImmutableArray<ProcessInfo>> GetAttachableProcessesAsync(CancellationToken ct)
        {
            var processes = await _processListAggregator.GetAttachableProcessesAsync(ct);
            foreach (var process in processes)
                await process.Initialize(ct);
            return processes;
        }
    }
}
