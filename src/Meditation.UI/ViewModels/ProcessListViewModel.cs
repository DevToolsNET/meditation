using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meditation.Common.Models;
using Meditation.Common.Services;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.UI.ViewModels
{
    public partial class ProcessListViewModel : ViewModelBase
    {
        private readonly IAttachableProcessListProvider processListProvider;

        [ObservableProperty] private Task<ImmutableArray<ProcessInfo>> processList;
        [ObservableProperty] private ProcessInfo? selectedProcess;
        [ObservableProperty] private string? nameFilter;

        public ProcessListViewModel(IAttachableProcessListProvider processListProvider)
        {
            this.processListProvider = processListProvider;
            ProcessList = GetAttachableProcessesAsync(CancellationToken.None);
        }

        [RelayCommand]
        public void FilterProcessList()
        {
            var filteredProcesses  = GetAttachableProcessesAsync(CancellationToken.None)
                .ContinueWith(previous => previous.Result.Where(p => NameFilter == null || p.Name.Contains(NameFilter)).ToImmutableArray());
            ProcessList = filteredProcesses;
        }

        [RelayCommand]
        public void RefreshProcessList()
        {
            processListProvider.Refresh();
            ProcessList = GetAttachableProcessesAsync(CancellationToken.None);
        }

        private async Task<ImmutableArray<ProcessInfo>> GetAttachableProcessesAsync(CancellationToken ct)
        {
            var processes = await processListProvider.GetAllAttachableProcessesAsync(ct);
            foreach (var process in processes)
                await process.Initialize(ct);
            return processes;
        }
    }
}
