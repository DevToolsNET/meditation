using CliWrap;
using Meditation.Common.Models;
using Meditation.Common.Services;
using Microsoft.Diagnostics.NETCore.Client;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.Core.Services
{
    internal class AttachableProcessListProvider : IAttachableProcessListProvider
    {
        private readonly IProcessListProvider processListProvider;
        private Task<ImmutableArray<ProcessInfo>> attachableProcessesTask;

        public AttachableProcessListProvider(IProcessListProvider processListProvider)
        {
            this.processListProvider = processListProvider;
            attachableProcessesTask = LoadAttachableProcessesAsync();
        }

        public Task<ImmutableArray<ProcessInfo>> GetAllAttachableProcessesAsync(CancellationToken ct)
            => attachableProcessesTask;

        public void Refresh()
        {
            processListProvider.Refresh();
            attachableProcessesTask = LoadAttachableProcessesAsync();
        }

        private async Task<ImmutableArray<ProcessInfo>> LoadAttachableProcessesAsync()
        {
            var builder = new List<ProcessInfo>();
            var netCoreProcesses = LoadNetCoreProcesses();
            var netCoreProcessesLookup = netCoreProcesses.ToDictionary(p => p.Id, p => p);

            foreach (var netFrameworkProcess in await LoadNetFrameworkProcessesAsync())
            {
                // NET Framework process obtaining logic might not be 100% reliable, trust .NET Core client primarily
                if (netCoreProcessesLookup.ContainsKey(netFrameworkProcess.Id))
                    continue;

                builder.Add(netFrameworkProcess);
            }

            builder.AddRange(netCoreProcessesLookup.Values);
            return builder.ToImmutableArray();
        }

        private async Task<ImmutableArray<ProcessInfo>> LoadNetFrameworkProcessesAsync()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                ImmutableArray.Create<ProcessInfo>();

            var builder = new List<ProcessInfo>();
            var stdout = new StringBuilder();
            await Cli.Wrap("cmd")
                .WithArguments("/c \"tasklist /m \"mscorlib*\" /fo csv /nh\"")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
                .ExecuteAsync();

            string? currentLine;
            using var textReader = new StringReader(stdout.ToString());
            while ((currentLine = await textReader.ReadLineAsync()) != null)
            {
                var tokens = currentLine.Split(',');
                var rawPid = tokens[1].Trim('\"');
                if (!int.TryParse(rawPid, out var pid) || !processListProvider.TryGetProcessById(pid, out _))
                    continue;

                builder.Add(ProcessInfo.CreateFrom(processListProvider.GetProcessById(pid), ProcessType.NetFramework));
            }

            return builder.ToImmutableArray();
        }

        private IEnumerable<ProcessInfo> LoadNetCoreProcesses()
        {
            var attachableProcessIds = DiagnosticsClient.GetPublishedProcesses();
            return attachableProcessIds
                .Where(id => processListProvider.TryGetProcessById(id, out _))
                .Select(pid => ProcessInfo.CreateFrom(processListProvider.GetProcessById(pid), ProcessType.NetCoreApp));
        }
    }
}
