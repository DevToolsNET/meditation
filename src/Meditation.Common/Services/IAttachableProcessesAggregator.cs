using Meditation.Common.Models;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Threading;

namespace Meditation.Common.Services
{
    public interface IAttachableProcessesAggregator
    {
        Task<ImmutableArray<ProcessInfo>> GetAttachableProcessesAsync(CancellationToken ct);

        void Refresh();
    }
}
