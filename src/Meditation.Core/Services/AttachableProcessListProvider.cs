using Meditation.Common.Models;
using Meditation.Common.Services;
using Microsoft.Diagnostics.NETCore.Client;
using System.Collections.Immutable;

namespace Meditation.Core.Services
{
    internal class AttachableProcessListProvider : IAttachableProcessListProvider
    {
        private readonly ImmutableArray<ProcessInfo> attachedProcesses;

        public AttachableProcessListProvider(IProcessListProvider processListProvider)
        {
            var attachableProcessIds = DiagnosticsClient.GetPublishedProcesses();
            attachedProcesses = attachableProcessIds
                .Where(id => processListProvider.TryGetProcessById(id, out _))
                .Select(processListProvider.GetProcessById)
                .ToImmutableArray();
        }

        public ImmutableArray<ProcessInfo> GetAllAttachableProcesses()
            => attachedProcesses;
    }
}
