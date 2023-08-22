using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService
{
    public interface IProcessArchitectureProvider
    {
        Task<bool> TryGetProcessArchitectureAsync(Process process, [NotNullWhen(returnValue: true)] out Architecture? architecture, CancellationToken ct);
    }
}
