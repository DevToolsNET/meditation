using Meditation.AttachProcessService.Models;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService
{
    public interface IAttachableProcessListProvider
    {
        ProcessType ProviderType { get; }
        Task<ImmutableArray<ProcessInfo>> GetAttachableProcessesAsync(CancellationToken ct);

        void Refresh();
    }
}
