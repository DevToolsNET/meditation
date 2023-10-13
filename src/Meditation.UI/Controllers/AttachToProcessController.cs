using CommunityToolkit.Mvvm.Input;
using Meditation.AttachProcessService.Models;
using Meditation.AttachProcessService;
using Meditation.UI.Services.Dialogs;
using Meditation.UI.ViewModels;
using Meditation.UI.Windows;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.UI.Controllers
{
    public partial class AttachToProcessController
    {
        private readonly IAvaloniaDialogService _dialogService;
        private readonly IProcessSnapshotCreator _processSnapshotCreator;
        private readonly IAttachedProcessContext _attachedProcessContext;
        private DialogLifetime? _attachProcessDialogLifetime;

        public AttachToProcessController(
            IAvaloniaDialogService dialogService,
            IAttachedProcessContext attachedProcessContext,
            IProcessSnapshotCreator processSnapshotCreator)
        {
            _dialogService = dialogService;
            _attachedProcessContext = attachedProcessContext;
            _processSnapshotCreator = processSnapshotCreator;
        }

        [RelayCommand]
        public async Task DisplayAttachProcessWindow()
        {
            if (_attachedProcessContext.ProcessSnapshot is { } snapshot)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    title: "Cannot attach to multiple processes",
                    text: $"To attach another process, detach first from the currently attached process ({snapshot.ProcessId.Value}).",
                    @enum: ButtonEnum.Ok);
                await messageBox.ShowAsync();
                return;
            }

            _attachProcessDialogLifetime = _dialogService.CreateDialog<AttachToProcessWindow>();
            _attachProcessDialogLifetime.Show();
        }

        [RelayCommand]
        public void CloseAttachProcessWindow()
        {
            if (_attachProcessDialogLifetime == null)
                throw new InvalidOperationException($"Dialog {nameof(AttachToProcessWindow)} is not opened.");

            _attachProcessDialogLifetime.Close();
            _attachProcessDialogLifetime.Dispose();
            _attachProcessDialogLifetime = null;
        }

        [RelayCommand]
        public async Task Attach(ProcessListViewModel processListViewModel, CancellationToken ct)
        {
            if (processListViewModel.SelectedProcess is not { } selectedProcessInfo)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    title: "Nothing selected",
                    text: "You need to select a process before proceeding.",
                    @enum: ButtonEnum.Ok);
                await messageBox.ShowAsync();
                return;
            }

            using var snapshot = await TryObtainProcessSnapshot(selectedProcessInfo, ct);
            if (snapshot == null)
                return;

            CloseAttachProcessWindow();
            await Task.Run(() => _attachedProcessContext.Initialize(snapshot), ct);
        }

        [RelayCommand]
        public async Task Detach(CancellationToken ct)
        {
            if (_attachedProcessContext.ProcessSnapshot is { } snapshot)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    title: "Preparing to detach process",
                    text: $"Do you want to detach from process ({snapshot.ProcessId.Value})?",
                    @enum: ButtonEnum.YesNo);

                if (await messageBox.ShowAsync() == ButtonResult.Yes)
                    await Task.Run(() => _attachedProcessContext.Reset(), ct);
            }
            else
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    title: "Nothing to detach",
                    text: "No process is currently attached.",
                    @enum: ButtonEnum.Ok);
                await messageBox.ShowAsync();
            }
        }

        private async Task<IProcessSnapshot?> TryObtainProcessSnapshot(ProcessInfo processInfo, CancellationToken ct)
        {
            try
            {
                return await _processSnapshotCreator.CreateProcessSnapshotAsync(processInfo.Id, ct);
            }
            catch (Exception exception)
            {
                // FIXME [#16]: add logging
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    title: "Failed to attach process",
                    text: $"Could not attach to selected process due to: {exception}",
                    @enum: ButtonEnum.Ok);
                await messageBox.ShowAsync();
                return null;
            }
        }
    }
}
