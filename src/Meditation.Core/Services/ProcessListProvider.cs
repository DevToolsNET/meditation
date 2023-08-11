using System;
using Meditation.Common.Models;
using Meditation.Common.Services;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace Meditation.Core.Services
{
    internal class ProcessListProvider : IProcessListProvider
    {
        private readonly IProcessCommandLineProvider commandLineArgumentsProvider;
        private readonly IProcessArchitectureProvider processArchitectureProvider;
        private ImmutableArray<ProcessInfo> processes;
        private ImmutableDictionary<int, ProcessInfo> processesLookup;

        public ProcessListProvider(IProcessCommandLineProvider commandLineArgumentsProvider, IProcessArchitectureProvider processArchitectureProvider)
        {
            this.commandLineArgumentsProvider = commandLineArgumentsProvider;
            this.processArchitectureProvider = processArchitectureProvider;

            processes = LoadProcesses();
            processesLookup = LoadProcessLookup(processes);
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

        public void Refresh()
        {
            processes = LoadProcesses();
            processesLookup = LoadProcessLookup(processes);
        }

        private ImmutableArray<ProcessInfo> LoadProcesses()
        {
            return Process.GetProcesses()
                .Select(process => new ProcessInfo(
                    process,
                    ProcessType.Unknown,
                    ConstructLazyCommandLineArguments(process, commandLineArgumentsProvider),
                    ConstructLazyProcessArchitecture(process, processArchitectureProvider)))
                .ToImmutableArray();
        }

        private ImmutableDictionary<int, ProcessInfo> LoadProcessLookup(ImmutableArray<ProcessInfo> currentProcesses)
        {
            return currentProcesses
                .ToDictionary(p => p.Id, p => p)
                .ToImmutableDictionary();
        }

        private static Lazy<string?> ConstructLazyCommandLineArguments(Process process, IProcessCommandLineProvider commandLineArgumentsProvider)
        {
            return new Lazy<string?>(() =>
            {
                commandLineArgumentsProvider.TryGetCommandLineArguments(process, out var commandLineArguments);
                return commandLineArguments;
            });
        }

        private static Lazy<Architecture?> ConstructLazyProcessArchitecture(Process process, IProcessArchitectureProvider processArchitectureProvider)
        {
            return new Lazy<Architecture?>(() =>
            {
                processArchitectureProvider.TryGetProcessArchitecture(process, out var architecture);
                return architecture;
            });
        }
    }
}
