using System;
using Meditation.AttachProcessService.Models;
using Microsoft.Diagnostics.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService.Services
{
    internal class ProcessSnapshotCreator : IProcessSnapshotCreator
    {
        public Task<IProcessSnapshot> CreateProcessSnapshotAsync(ProcessId processId, CancellationToken ct)
        {
            return Task.Run<IProcessSnapshot>(() =>
            {
                var dataTarget = DataTarget.CreateSnapshotAndAttach(processId.Value);
                if (dataTarget.ClrVersions.IsEmpty)
                    throw new ArgumentException($"Target process ({processId}) does not have a .NET runtime", nameof(processId));

                return new ProcessSnapshot(processId, dataTarget);
            }, ct);
        }
    }
}