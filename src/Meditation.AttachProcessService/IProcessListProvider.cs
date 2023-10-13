using Meditation.AttachProcessService.Models;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Meditation.AttachProcessService
{
    public interface IProcessListProvider
    {
        ImmutableArray<ProcessInfo> GetAllProcesses();
        bool TryGetProcessById(ProcessId pid, [NotNullWhen(true)] out ProcessInfo? processInfo);
        ProcessInfo GetProcessById(ProcessId pid);
        void Refresh();
    }
}
