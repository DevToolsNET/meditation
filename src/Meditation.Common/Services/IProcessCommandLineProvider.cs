using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.Common.Services
{
    public interface IProcessCommandLineProvider
    {
        Task<bool> TryGetCommandLineArgumentsAsync(Process process, [NotNullWhen(returnValue: true)] out string? commandLineArguments, CancellationToken ct);
    }
}
