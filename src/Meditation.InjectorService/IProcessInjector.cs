using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Meditation.InjectorService
{
    public interface IProcessInjector
    {
        bool TryInjectModule(int pid, string assemblyPath, [NotNullWhen(true)] out SafeHandle? moduleHandle);
    }
}
