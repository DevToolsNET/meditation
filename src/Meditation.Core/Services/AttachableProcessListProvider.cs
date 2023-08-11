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

namespace Meditation.Core.Services
{
    internal class AttachableProcessListProvider : IAttachableProcessListProvider
    {
        private readonly IProcessListProvider processListProvider;
        private ImmutableArray<ProcessInfo> attachableProcesses;

        public AttachableProcessListProvider(IProcessListProvider processListProvider)
        {
            this.processListProvider = processListProvider;
            LoadAttachableProcesses();
        }

        public ImmutableArray<ProcessInfo> GetAllAttachableProcesses()
            => attachableProcesses;

        public void Refresh()
        {
            processListProvider.Refresh();
            LoadAttachableProcesses();
        }

        private void LoadAttachableProcesses()
        {
            var builder = new List<ProcessInfo>();
            var netCoreProcesses = LoadNetCoreProcesses();
            var netCoreProcessesLookup = netCoreProcesses.ToDictionary(p => p.Id, p => p);

            foreach (var netFrameworkProcess in LoadNetFrameworkProcesses())
            {
                // NET Framework process obtaining logic might not be 100% reliable, trust .NET Core client primarily
                if (netCoreProcessesLookup.ContainsKey(netFrameworkProcess.Id))
                    continue;

                builder.Add(netFrameworkProcess);
            }

            builder.AddRange(netCoreProcessesLookup.Values);
            attachableProcesses = builder.ToImmutableArray();
        }

        private IEnumerable<ProcessInfo> LoadNetFrameworkProcesses()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                yield break;

            var stdout = new StringBuilder();
            Cli.Wrap("cmd")
                .WithArguments("/c \"tasklist /m \"mscorlib*\" /fo csv /nh\"")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
                .ExecuteAsync()
                .GetAwaiter()
                .GetResult();

            using var textReader = new StringReader(stdout.ToString());
            string? currentLine;
            while ((currentLine = textReader.ReadLine()) != null)
            {
                var tokens = currentLine.Split(',');
                var rawPid = tokens[1].Trim('\"');
                if (!int.TryParse(rawPid, out var pid) || !processListProvider.TryGetProcessById(pid, out _))
                    continue;

                yield return processListProvider.GetProcessById(pid) with { Type = ProcessType.NetFramework };
            }
        }

        private IEnumerable<ProcessInfo> LoadNetCoreProcesses()
        {
            var attachableProcessIds = DiagnosticsClient.GetPublishedProcesses();
            return attachableProcessIds
                .Where(id => processListProvider.TryGetProcessById(id, out _))
                .Select(pid => processListProvider.GetProcessById(pid) with { Type = ProcessType.NetCoreApp });
        }
    }
}
