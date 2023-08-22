using Meditation.AttachProcessService.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;

namespace Meditation.AttachProcessService.Services.Windows
{
    internal class AttachableNetFrameworkProcessListProvider : IAttachableProcessListProvider
    {
        private const string CommandExecutableName = "cmd.exe";
        private const string CommandArguments = "/c \"tasklist /m \"mscorlib*\" /fo csv /nh\"";
        private readonly IProcessListProvider _processListProvider;
        private Task<ImmutableArray<ProcessInfo>> _attachableProcessesTask;

        public AttachableNetFrameworkProcessListProvider(IProcessListProvider processListProvider)
        {
            _processListProvider = processListProvider;
            _attachableProcessesTask = LoadNetFrameworkProcessesAsync();
        }

        public ProcessType ProviderType => ProcessType.NetFramework;

        public Task<ImmutableArray<ProcessInfo>> GetAttachableProcessesAsync(CancellationToken ct) => _attachableProcessesTask;

        public void Refresh() => _attachableProcessesTask = LoadNetFrameworkProcessesAsync();

        private async Task<ImmutableArray<ProcessInfo>> LoadNetFrameworkProcessesAsync()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                ImmutableArray.Create<ProcessInfo>();

            var stdout = new StringBuilder();
            await Cli.Wrap(CommandExecutableName)
                .WithArguments(CommandArguments)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
                .ExecuteAsync();

            return ParseProcessInformation(stdout.ToString());
        }

        private ImmutableArray<ProcessInfo> ParseProcessInformation(string commandStdout)
        {
            var builder = new List<ProcessInfo>();

            string? currentLine;
            using var textReader = new StringReader(commandStdout);
            while ((currentLine = textReader.ReadLine()) != null)
            {
                var tokens = currentLine.Split(',');
                var rawPid = tokens[1].Trim('\"');
                if (!int.TryParse(rawPid, out var pid) || !_processListProvider.TryGetProcessById(pid, out _))
                    continue;

                builder.Add(ProcessInfo.CreateFrom(_processListProvider.GetProcessById(pid), ProcessType.NetFramework));
            }

            return builder.ToImmutableArray();
        }
    }
}
