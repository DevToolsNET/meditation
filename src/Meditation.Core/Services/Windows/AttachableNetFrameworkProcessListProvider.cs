using CliWrap;
using Meditation.Common.Models;
using Meditation.Common.Services;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.Core.Services.Windows
{
    internal class AttachableNetFrameworkProcessListProvider : IAttachableProcessListProvider
    {
        private const string CommandExecutableName = "cmd.exe";
        private const string CommandArguments = "/c \"tasklist /m \"mscorlib*\" /fo csv /nh\"";
        private readonly IProcessListProvider processListProvider;
        private Task<ImmutableArray<ProcessInfo>> attachableProcessesTask;

        public AttachableNetFrameworkProcessListProvider(IProcessListProvider processListProvider)
        {
            this.processListProvider = processListProvider;
            attachableProcessesTask = LoadNetFrameworkProcessesAsync();
        }

        public ProcessType ProviderType => ProcessType.NetFramework;

        public Task<ImmutableArray<ProcessInfo>> GetAttachableProcessesAsync(CancellationToken ct)
            => attachableProcessesTask;

        public void Refresh()
            => attachableProcessesTask = LoadNetFrameworkProcessesAsync();

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
                if (!int.TryParse(rawPid, out var pid) || !processListProvider.TryGetProcessById(pid, out _))
                    continue;

                builder.Add(ProcessInfo.CreateFrom(processListProvider.GetProcessById(pid), ProcessType.NetFramework));
            }

            return builder.ToImmutableArray();
        }
    }
}
