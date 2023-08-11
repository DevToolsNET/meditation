using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Meditation.Common.Services
{
    public interface IProcessCommandLineProvider
    {
        bool TryGetCommandLineArguments(Process process, [NotNullWhen(returnValue: true)] out string? commandLineArguments);
    }
}
