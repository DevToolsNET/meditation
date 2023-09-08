using CommunityToolkit.Mvvm.Input;
using Meditation.AttachProcessService;
using Meditation.UI.Windows;
using System;
using System.Threading;
using System.Threading.Tasks;
using Meditation.AttachProcessService.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

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
            
            using var snapshot = await TryObtainProcessSnapshot(selectedProcessInfo, ct);
            if (snapshot == null)
                return;

            await _dialogsContext.CloseDialogAsync<AttachToProcessWindow>();
            await Task.Run(() => _attachedProcessController.Attach(snapshot), ct);
        }

        private async Task<IProcessSnapshot?> TryObtainProcessSnapshot(ProcessInfo processInfo, CancellationToken ct)
        {
            try
            {
                return await _processSnapshotCreator.CreateProcessSnapshotAsync(processInfo.Id, ct);
            }
            catch (Exception e)
            {
                // FIXME: add logging
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    title: "Failed to attach process",
                    text: $"Could not attach to selected process due to: {e.Message}",
                    @enum: ButtonEnum.Ok);
                await messageBox.ShowAsync();
                return null;
            }
        }
    }
}
