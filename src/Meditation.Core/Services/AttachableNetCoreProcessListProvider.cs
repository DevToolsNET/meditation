using Meditation.Common.Models;
using Meditation.Common.Services;
using Microsoft.Diagnostics.NETCore.Client;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.Core.Services
{
    internal class AttachableNetCoreProcessListProvider : IAttachableProcessListProvider
    {
        private readonly IProcessListProvider processListProvider;
        private ImmutableArray<ProcessInfo> attachableProcesses;

        public AttachableNetCoreProcessListProvider(IProcessListProvider processListProvider)
        {
            this.processListProvider = processListProvider;
            attachableProcesses = LoadAttachableProcesses().ToImmutableArray();
        }

        public ProcessType ProviderType => ProcessType.NetCoreApp;

        public Task<ImmutableArray<ProcessInfo>> GetAttachableProcessesAsync(CancellationToken ct)
            => Task.FromResult(attachableProcesses);

        public void Refresh()
            => attachableProcesses = LoadAttachableProcesses().ToImmutableArray();

        private IEnumerable<ProcessInfo> LoadAttachableProcesses()
        {
            var attachableProcessIds = DiagnosticsClient.GetPublishedProcesses();
            return attachableProcessIds
                .Where(id => processListProvider.TryGetProcessById(id, out _))
                .Select(pid => ProcessInfo.CreateFrom(processListProvider.GetProcessById(pid), ProcessType.NetCoreApp));
        }
    }
}
