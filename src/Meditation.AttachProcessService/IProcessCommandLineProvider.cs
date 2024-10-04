using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService
{
    public interface IProcessCommandLineProvider
    {
        Task<string?> TryGetCommandLineArgumentsAsync(Process process, CancellationToken ct);
    }
}
