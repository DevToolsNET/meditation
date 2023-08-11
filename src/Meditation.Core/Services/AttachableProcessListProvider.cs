using Meditation.Common.Models;
using Meditation.Common.Services;
using Microsoft.Diagnostics.NETCore.Client;
using System.Collections.Immutable;
using System.Linq;

namespace Meditation.Core.Services
{
    internal class AttachableProcessListProvider : IAttachableProcessListProvider
    {
        private readonly IProcessListProvider processListProvider;
        private ImmutableArray<ProcessInfo> attachedProcesses;

        public AttachableProcessListProvider(IProcessListProvider processListProvider)
        {
            this.processListProvider = processListProvider;
            LoadAttachableProcesses();
        }

        public ImmutableArray<ProcessInfo> GetAllAttachableProcesses()
            => attachedProcesses;

        public void Refresh()
        {
            processListProvider.Refresh();
            LoadAttachableProcesses();
        }

        private void LoadAttachableProcesses()
        {
            var attachableProcessIds = DiagnosticsClient.GetPublishedProcesses();
            attachedProcesses = attachableProcessIds
                .Where(id => processListProvider.TryGetProcessById(id, out _))
                .Select(pid => processListProvider.GetProcessById(pid) with { Type = ProcessType.NetCoreApp })
                .ToImmutableArray();
        }
    }
}
