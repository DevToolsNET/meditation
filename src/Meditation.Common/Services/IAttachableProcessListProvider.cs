using System.Collections.Immutable;
using System.Threading.Tasks;
using Meditation.Common.Models;

namespace Meditation.Common.Services
{
    public interface IAttachableProcessListProvider
    {
        Task<ImmutableArray<ProcessInfo>> GetAllAttachableProcessesAsync();

        void Refresh();
    }
}
