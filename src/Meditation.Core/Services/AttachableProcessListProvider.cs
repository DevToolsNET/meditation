using Meditation.Common.Models;
using Meditation.Common.Services;
using Microsoft.Diagnostics.NETCore.Client;
using System.Collections.Immutable;
using System.Linq;

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
                .Select(pid => new ProcessInfo(
                    processListProvider.GetProcessById(pid).ProcessInternal, 
                    isNetCoreApp: true, 
                    isNetFramework: false))
                .ToImmutableArray();
        }

        public ImmutableArray<ProcessInfo> GetAllAttachableProcesses()
            => attachedProcesses;
    }
}
