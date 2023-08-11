using System;
using Meditation.Common.Models;
using Meditation.Common.Services;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Meditation.Core.Services
{
    internal class ProcessListProvider : IProcessListProvider
    {
        private readonly ImmutableArray<ProcessInfo> processes;
        private readonly ImmutableDictionary<int, ProcessInfo> processesLookup;

        public ProcessListProvider(ICommandLineArgumentsProvider commandLineArgumentsProvider)
        {
            processes = Process.GetProcesses()
                .Select(process => new ProcessInfo(process, ProcessType.Unknown, ConstructLazyCommandLineArguments(process, commandLineArgumentsProvider)))
                .ToImmutableArray();

            processesLookup = processes
                .ToDictionary(p => p.Id, p => p)
                .ToImmutableDictionary();
        }

        public ImmutableArray<ProcessInfo> GetAllProcesses()
            => processes;

        public bool TryGetProcessById(int pid, [NotNullWhen(true)] out ProcessInfo? processInfo)
            => processesLookup.TryGetValue(pid, out processInfo);

        public ProcessInfo GetProcessById(int pid)
        {
            if (TryGetProcessById(pid, out var processInfo))
                return processInfo;

            throw new ArgumentException("Unable to find process with specified id.", nameof(pid));
        }

        private static Lazy<string?> ConstructLazyCommandLineArguments(Process process, ICommandLineArgumentsProvider commandLineArgumentsProvider)
        {
            return new Lazy<string?>(() =>
            {
                commandLineArgumentsProvider.TryGetCommandLineArguments(process, out var commandLineArguments);
                return commandLineArguments;
            });
        }
    }
}
