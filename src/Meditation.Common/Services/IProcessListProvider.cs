using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Meditation.Common.Models;

namespace Meditation.Common.Services
{
    public interface IProcessListProvider
    {
        ImmutableArray<ProcessInfo> GetAllProcesses();
        bool TryGetProcessById(int pid, [NotNullWhen(true)] out ProcessInfo? processInfo);
        ProcessInfo GetProcessById(int pid);
    }
}
