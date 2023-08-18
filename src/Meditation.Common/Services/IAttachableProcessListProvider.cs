using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Meditation.Common.Models;

namespace Meditation.Common.Services
{
    public interface IAttachableProcessListProvider
    {
        Task<ImmutableArray<ProcessInfo>> GetAllAttachableProcessesAsync(CancellationToken ct);

        void Refresh();
    }
}
