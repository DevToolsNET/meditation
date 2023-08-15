using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meditation.Common.Models;
using Meditation.Common.Services;
using System.Collections.ObjectModel;
using System.Linq;
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
            ProcessList = LoadAttachableProcessesAsync();
        }

        [RelayCommand]
        public void FilterProcessList()
        {
            var filteredProcesses  = LoadAttachableProcessesAsync()
                .ContinueWith(previous => previous.Result.Where(p => NameFilter == null || p.Name.Contains(NameFilter)).ToImmutableArray());
            ProcessList = filteredProcesses;
        }

        [RelayCommand]
        public void RefreshProcessList()
        {
            processListProvider.Refresh();
            ProcessList = LoadAttachableProcessesAsync();
        }

        private Task<ImmutableArray<ProcessInfo>> LoadAttachableProcessesAsync()
            => processListProvider.GetAllAttachableProcessesAsync();
    }
}
