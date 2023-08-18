using System;
using Meditation.Common.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.Common.Models
{
    public class ProcessInfo
    {
        public int Id { get; }
        public string Name { get; }
        public ProcessType Type { get; }
        public Architecture? Architecture { get; private set; }
        public string? CommandLineArguments { get; private set; }
        internal readonly Process InternalProcess;
        private readonly IProcessCommandLineProvider processCommandLineProvider;
        private readonly IProcessArchitectureProvider processArchitectureProvider;
        private bool isInitialized;

        public ProcessInfo(Process process, ProcessType type, IProcessCommandLineProvider commandLineProvider, IProcessArchitectureProvider architectureProvider)
        {
            Id = process.Id;
            Name = process.ProcessName;
            Type = type;
            processCommandLineProvider = commandLineProvider;
            processArchitectureProvider = architectureProvider;
            InternalProcess = process;
        }

        public async Task Initialize(CancellationToken ct)
        {
            if (isInitialized)
                return;

            if (!await processArchitectureProvider.TryGetProcessArchitectureAsync(InternalProcess, out var architecture, ct))
                throw new Exception($"Could not obtain process architecture for PID={Id}");

            if (!await processCommandLineProvider.TryGetCommandLineArgumentsAsync(InternalProcess, out var commandLineArguments, ct))
                throw new Exception($"Could not obtain process command line arguments for PID={Id}");

            Architecture = architecture;
            CommandLineArguments = commandLineArguments;
            isInitialized = true;
        }

        public static ProcessInfo CreateFrom(ProcessInfo processInfo, ProcessType newProcessType)
        {
            return new ProcessInfo(
                processInfo.InternalProcess,
                newProcessType,
                processInfo.processCommandLineProvider,
                processInfo.processArchitectureProvider)
            {
                Architecture = processInfo.Architecture,
                CommandLineArguments = processInfo.CommandLineArguments
            };
        }

        public override string ToString()
            => InternalProcess.ToString();
    }
}
