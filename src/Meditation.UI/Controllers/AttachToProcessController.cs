using CommunityToolkit.Mvvm.Input;
using Meditation.AttachProcessService.Models;
using Meditation.AttachProcessService;
using Meditation.UI.Services.Dialogs;
using Meditation.UI.ViewModels;
using Meditation.UI.Windows;
using MsBox.Avalonia.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using Meditation.UI.Controllers.Utils;

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
        public Task DisplayAttachProcessWindow()
        {
            try
            {
                return DisplayAttachProcessWindowImplementation();
            }
            catch (Exception exception)
            {
                return DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        [RelayCommand]
        public Task CloseAttachProcessWindow()
        {
            try
            {
                CloseAttachProcessWindowImplementation();
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        [RelayCommand]
        public Task Attach(ProcessListViewModel processListViewModel, CancellationToken ct)
        {
            try
            {
                return AttachImplementation(processListViewModel, ct);
            }
            catch (Exception exception)
            {
                return DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        [RelayCommand]
        public Task Detach(CancellationToken ct)
        {
            try
            {
                return DetachImplementation(ct);
            }
            catch (Exception exception)
            {
                return DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        private async Task DisplayAttachProcessWindowImplementation()
        {
            if (_attachedProcessContext.ProcessSnapshot is { } snapshot)
            {
                await DialogUtilities.ShowMessageBox(
                    title: "Cannot attach to multiple processes",
                    content: $"To attach another process, detach first from the currently attached process ({snapshot.ProcessId.Value}).");
                return;
            }

            _attachProcessDialogLifetime = _dialogService.CreateDialog<AttachToProcessWindow>();
            await _attachProcessDialogLifetime.Show();
        }

        private void CloseAttachProcessWindowImplementation()
        {
            if (_attachProcessDialogLifetime == null)
                throw new InvalidOperationException($"Dialog {nameof(AttachToProcessWindow)} is not opened.");

            _attachProcessDialogLifetime.Close();
            _attachProcessDialogLifetime.Dispose();
            _attachProcessDialogLifetime = null;
        }

        private async Task AttachImplementation(ProcessListViewModel processListViewModel, CancellationToken ct)
        {
            if (processListViewModel.SelectedProcess is not { } selectedProcessInfo)
            {
                await DialogUtilities.ShowMessageBox(title: "Nothing selected", content: "You need to select a process before proceeding.");
                return;
            }

            using var snapshot = await TryObtainProcessSnapshot(selectedProcessInfo, ct);
            if (snapshot == null)
                return;

            CloseAttachProcessWindowImplementation();
            await Task.Run(() => _attachedProcessContext.Initialize(snapshot), ct);
        }

        private async Task DetachImplementation(CancellationToken ct)
        {
            if (_attachedProcessContext.ProcessSnapshot is { } snapshot)
            {
                var result = await DialogUtilities.ShowMessageBox(
                    title: "Preparing to detach process",
                    content: $"Do you want to detach from process ({snapshot.ProcessId.Value})?",
                    buttonEnum: ButtonEnum.YesNo);

                if (result == ButtonResult.Yes)
                    await Task.Run(() => _attachedProcessContext.Reset(), ct);
            }
            else
            {
                await DialogUtilities.ShowMessageBox(title: "Nothing to detach", content: "No process is currently attached.");
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
                await DialogUtilities.ShowMessageBox(title: "Failed to attach process", content: $"Could not attach to selected process due to: {exception}");
                return null;
            }
        }
    }
}
