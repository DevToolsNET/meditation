using Meditation.Common.Models;
using Meditation.Common.Services;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Meditation.Core.Services
{
    internal class ProcessListProvider : IProcessListProvider
    {
        private readonly ImmutableArray<ProcessInfo> processes;
        private readonly ImmutableDictionary<int, ProcessInfo> processesLookup;

        public ProcessListProvider()
        {
            processes = Process.GetProcesses().Select(p => new ProcessInfo(p)).ToImmutableArray();
            processesLookup = processes.ToDictionary(p => p.Id, p => p).ToImmutableDictionary();
        }

        public ImmutableArray<ProcessInfo> GetAllProcesses()
            => processes;

        public ProcessInfo GetProcessById(int pid)
        {
            if (TryGetProcessById(pid, out var processInfo))
                return processInfo;

            throw new ArgumentException("Unable to find process with specified id.", nameof(pid));
        }

        public bool TryGetProcessById(int pid, [NotNullWhen(true)] out ProcessInfo? processInfo)
            => processesLookup.TryGetValue(pid, out processInfo);
    }
}
