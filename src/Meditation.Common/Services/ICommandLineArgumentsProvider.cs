using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Meditation.Common.Services
{
    public interface ICommandLineArgumentsProvider
    {
        bool TryGetCommandLineArguments(Process process, [NotNullWhen(returnValue: true)] out string? commandLineArguments);
    }
}
