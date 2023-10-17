using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Meditation.InjectorService
{
    public interface IProcessInjector
    {
        bool TryInjectModuleToProcess(int pid, string assemblyPath, [NotNullWhen(true)] out SafeHandle? moduleHandle);
        bool TryExecuteStaticMethodInDefaultProcessAppDomain(int pid, string assemblyPath, string fullTypeName, string methodName, string arg, out int returnCode);
    }
}
