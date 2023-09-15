using Meditation.AttachProcessService.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService
{
    public interface IProcessSnapshotCreator
    {
        Task<IProcessSnapshot> CreateProcessSnapshotAsync(ProcessId processId, CancellationToken ct);
    }
}
