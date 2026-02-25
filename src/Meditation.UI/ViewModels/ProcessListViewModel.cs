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
        private readonly IPrivilegeElevatorService _privilegeElevatorService;

        [ObservableProperty] private FilterableCollectionView<ProcessInfo> _processList;
        [ObservableProperty] private ProcessInfo? _selectedProcess;
        [ObservableProperty] private string? _nameFilter;
        [ObservableProperty] private bool _isElevated;

        public ProcessListViewModel(
            IAttachableProcessesAggregator processListAggregator,
            IPrivilegeElevatorService privilegeElevatorService)
        {
            _processListAggregator = processListAggregator;
            _privilegeElevatorService = privilegeElevatorService;

            IsElevated = _privilegeElevatorService.IsElevated();
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

        [RelayCommand]
        public Task RestartAsElevated()
        {
            return _privilegeElevatorService.RestartAsElevated();
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
