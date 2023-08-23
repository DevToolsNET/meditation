using Meditation.Common.Models;
using Meditation.Common.Services;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Meditation.Core.Services
{
    internal class ProcessListProvider : IProcessListProvider
    {
        private readonly IProcessCommandLineProvider _commandLineArgumentsProvider;
        private readonly IProcessArchitectureProvider _processArchitectureProvider;
        private ImmutableArray<ProcessInfo> _processes;
        private ImmutableDictionary<int, ProcessInfo> _processesLookup;

        public ProcessListProvider(IProcessCommandLineProvider commandLineArgumentsProvider, IProcessArchitectureProvider processArchitectureProvider)
        {
            _commandLineArgumentsProvider = commandLineArgumentsProvider;
            _processArchitectureProvider = processArchitectureProvider;

            _processes = LoadProcesses();
            _processesLookup = LoadProcessLookup(_processes);
        }

        public ImmutableArray<ProcessInfo> GetAllProcesses() => _processes;

        public bool TryGetProcessById(int pid, [NotNullWhen(true)] out ProcessInfo? processInfo) => _processesLookup.TryGetValue(pid, out processInfo);

        public ProcessInfo GetProcessById(int pid)
        {
            if (TryGetProcessById(pid, out var processInfo))
                return processInfo;

            throw new ArgumentException("Unable to find process with specified id.", nameof(pid));
        }

        public void Refresh()
        {
            _processes = LoadProcesses();
            _processesLookup = LoadProcessLookup(_processes);
        }

        private ImmutableArray<ProcessInfo> LoadProcesses()
        {
            return Process.GetProcesses()
                .Select(process => new ProcessInfo(
                    process,
                    ProcessType.Unknown,
                    _commandLineArgumentsProvider,
                    _processArchitectureProvider))
                .ToImmutableArray();
        }

        private ImmutableDictionary<int, ProcessInfo> LoadProcessLookup(ImmutableArray<ProcessInfo> currentProcesses)
        {
            return currentProcesses
                .ToDictionary(p => p.Id, p => p)
                .ToImmutableDictionary();
        }
    }
}
