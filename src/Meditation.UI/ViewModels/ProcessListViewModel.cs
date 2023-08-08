using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meditation.Common.Models;
using Meditation.Common.Services;
using System.Collections.ObjectModel;
using System.Linq;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Meditation.UI.ViewModels
{
    public partial class ProcessListViewModel : ViewModelBase
    {
        private readonly IAttachableProcessListProvider processListProvider;

        [ObservableProperty] private ObservableCollection<ProcessInfo> processList;
        [ObservableProperty] private ProcessInfo? selectedProcess;
        [ObservableProperty] private string? nameFilter;

        public ProcessListViewModel(IAttachableProcessListProvider processListProvider)
        {
            this.processListProvider = processListProvider;
            ProcessList = new ObservableCollection<ProcessInfo>(processListProvider.GetAllAttachableProcesses());
        }

        [RelayCommand]
        public void FilterProcessList()
        {
            var processes = processListProvider.GetAllAttachableProcesses();
            var filteredEnumerable = processes.Where(p => NameFilter == null || p.Name.Contains(NameFilter));
            ProcessList = new ObservableCollection<ProcessInfo>(filteredEnumerable);
        }

        [RelayCommand]
        public void RefreshProcessList()
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                title: "Missing feature",
                text: "This functionality is not yet implemented",
                @enum: ButtonEnum.Ok);
            messageBox.ShowAsync();
        }
    }
}
