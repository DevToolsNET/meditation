using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService
{
    public interface IProcessArchitectureProvider
    {
        Task<Architecture?> TryGetProcessArchitectureAsync(Process process, CancellationToken ct);
    }
}
