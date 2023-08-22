using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meditation.AttachProcessService.Models;

namespace Meditation.AttachProcessService.Services
{
    internal class AttachableProcessListAggregator : IAttachableProcessesAggregator
    {
        private readonly IProcessListProvider _genericProcessListProvider;
        private readonly ImmutableArray<IAttachableProcessListProvider> _dotnetProcessListProviders;
        private Task<ImmutableArray<ProcessInfo>> _attachableProcessesTask;

        public AttachableProcessListAggregator(
            IProcessListProvider genericProcessListProvider, 
            IEnumerable<IAttachableProcessListProvider> dotnetProcessListProviders)
        {
            _genericProcessListProvider = genericProcessListProvider;
            _dotnetProcessListProviders = dotnetProcessListProviders.ToImmutableArray();
            _attachableProcessesTask = LoadAttachableProcessesAsync(CancellationToken.None);
        }

        public Task<ImmutableArray<ProcessInfo>> GetAttachableProcessesAsync(CancellationToken ct) => _attachableProcessesTask;

        private async Task<ImmutableArray<ProcessInfo>> LoadAttachableProcessesAsync(CancellationToken ct)
        {
            // If there is .NET Core process list provider, prioritize it (more reliable API to obtain them)
            var netCoreProvider = _dotnetProcessListProviders.FirstOrDefault(p => p.ProviderType == ProcessType.NetCoreApp);
            if (netCoreProvider is not null)
            {
                var builder = new List<ProcessInfo>();
                var addedProcessIds = new HashSet<int>();
                var netCoreProcesses = await netCoreProvider.GetAttachableProcessesAsync(ct);
                foreach (var process in netCoreProcesses)
                {
                    builder.Add(process);
                    addedProcessIds.Add(process.Id);
                }

                // Load processes from other providers if they are not already recognized by .NET Core provider
                foreach (var provider in _dotnetProcessListProviders.Where(p => p != netCoreProvider))
                {
                    var processes = await provider.GetAttachableProcessesAsync(ct);
                    foreach (var process in processes.Where(p => !addedProcessIds.Contains(p.Id)))
                    {
                        builder.Add(process);
                        addedProcessIds.Add(process.Id);
                    }
                }

                return builder.ToImmutableArray();
            }
            // Otherwise just merge them
            else
            {
                var builder = new List<ProcessInfo>();
                var addedProcessIds = new HashSet<int>();
                foreach (var attachableProcessListProvider in _dotnetProcessListProviders)
                {
                    var processes = await attachableProcessListProvider.GetAttachableProcessesAsync(ct);
                    foreach (var process in processes.Where(p => !addedProcessIds.Contains(p.Id)))
                    {
                        builder.Add(process);
                        addedProcessIds.Add(process.Id);
                    }
                }

                return builder.ToImmutableArray();
            }
        }

        public void Refresh()
        {
            _genericProcessListProvider.Refresh();
            foreach (var provider in _dotnetProcessListProviders)
                provider.Refresh();
            
            _attachableProcessesTask = LoadAttachableProcessesAsync(CancellationToken.None);
        }
    }
}
