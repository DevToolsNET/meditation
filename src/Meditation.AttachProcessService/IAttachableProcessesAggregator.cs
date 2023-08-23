using Meditation.AttachProcessService.Models;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService
{
    public interface IAttachableProcessesAggregator
    {
        Task<ImmutableArray<ProcessInfo>> GetAttachableProcessesAsync(CancellationToken ct);

        void Refresh();
    }
}
