using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meditation.AttachProcessService.Models;
using Microsoft.Diagnostics.NETCore.Client;

namespace Meditation.AttachProcessService.Services
{
    internal class AttachableNetCoreProcessListProvider : IAttachableProcessListProvider
    {
        private readonly IProcessListProvider _processListProvider;
        private ImmutableArray<ProcessInfo> _attachableProcesses;

        public AttachableNetCoreProcessListProvider(IProcessListProvider processListProvider)
        {
            _processListProvider = processListProvider;
            _attachableProcesses = LoadAttachableProcesses().ToImmutableArray();
        }

        public ProcessType ProviderType => ProcessType.NetCoreApp;

        public Task<ImmutableArray<ProcessInfo>> GetAttachableProcessesAsync(CancellationToken ct) => Task.FromResult(_attachableProcesses);

        public void Refresh() => _attachableProcesses = LoadAttachableProcesses().ToImmutableArray();

        private IEnumerable<ProcessInfo> LoadAttachableProcesses()
        {
            var attachableProcessIds = DiagnosticsClient.GetPublishedProcesses();
            return attachableProcessIds
                .Where(id => _processListProvider.TryGetProcessById(id, out _))
                .Select(pid => ProcessInfo.CreateFrom(_processListProvider.GetProcessById(pid), ProcessType.NetCoreApp));
        }
    }
}