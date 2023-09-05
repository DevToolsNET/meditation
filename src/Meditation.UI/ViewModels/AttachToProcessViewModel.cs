using CommunityToolkit.Mvvm.Input;
using Meditation.AttachProcessService;
using Meditation.UI.Windows;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.UI.ViewModels
{
    public partial class AttachToProcessViewModel : ViewModelBase
    {
        private readonly IProcessSnapshotCreator _processSnapshotCreator;
        private readonly IAttachedProcessController _attachedProcessController;
        private readonly IAvaloniaDialogsContext _dialogsContext;

        public AttachToProcessViewModel(
            IProcessSnapshotCreator processSnapshotCreator, 
            IAttachedProcessController attachedProcessController, 
            IAvaloniaDialogsContext dialogsContext)
        {
            _processSnapshotCreator = processSnapshotCreator;
            _attachedProcessController = attachedProcessController;
            _dialogsContext = dialogsContext;
        }

        [RelayCommand]
        public async Task AttachProcess(ProcessListViewModel processListViewModel, CancellationToken ct)
        {
            if (processListViewModel.SelectedProcess is not { } selectedProcessInfo)
                throw new ArgumentException("No process was selected for attach", nameof(processListViewModel));
            
            using var snapshot = await _processSnapshotCreator.CreateProcessSnapshotAsync(selectedProcessInfo.Id, ct);
            await _dialogsContext.CloseDialogAsync<AttachToProcessWindow>();
            await Task.Run(() => _attachedProcessController.Attach(snapshot), ct);
        }
    }
}
