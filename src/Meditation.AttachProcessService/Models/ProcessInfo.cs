using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService.Models
{
    public class ProcessInfo
    {
        public ProcessId Id { get; }
        public string Name { get; }
        public ProcessType Type { get; }
        public Architecture? Architecture { get; private set; }
        public string? CommandLineArguments { get; private set; }
        internal readonly Process InternalProcess;
        private readonly IProcessCommandLineProvider _processCommandLineProvider;
        private readonly IProcessArchitectureProvider _processArchitectureProvider;
        private bool _isInitialized;

        public ProcessInfo(Process process, ProcessType type, IProcessCommandLineProvider commandLineProvider, IProcessArchitectureProvider architectureProvider)
        {
            Id = new ProcessId(process.Id);
            Name = process.ProcessName;
            Type = type;
            _processCommandLineProvider = commandLineProvider;
            _processArchitectureProvider = architectureProvider;
            InternalProcess = process;
        }

        public async Task Initialize(CancellationToken ct)
        {
            if (_isInitialized)
                return;

            if (await _processArchitectureProvider.TryGetProcessArchitectureAsync(InternalProcess, out var architecture, ct))
                Architecture = architecture;
            if (await _processCommandLineProvider.TryGetCommandLineArgumentsAsync(InternalProcess, out var commandLineArguments, ct))
                CommandLineArguments = commandLineArguments;

            _isInitialized = true;
        }

        public static ProcessInfo CreateFrom(ProcessInfo processInfo, ProcessType newProcessType)
        {
            return new ProcessInfo(
                processInfo.InternalProcess,
                newProcessType,
                processInfo._processCommandLineProvider,
                processInfo._processArchitectureProvider)
            {
                Architecture = processInfo.Architecture,
                CommandLineArguments = processInfo.CommandLineArguments
            };
        }

        public override string ToString() => InternalProcess.ToString();
    }
}
