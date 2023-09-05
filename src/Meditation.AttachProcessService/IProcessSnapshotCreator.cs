using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService
{
    public interface IProcessSnapshotCreator
    {
        Task<IProcessSnapshot> CreateProcessSnapshotAsync(int processId, CancellationToken ct);
    }
}
