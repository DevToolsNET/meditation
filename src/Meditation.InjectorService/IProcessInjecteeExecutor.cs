using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Meditation.InjectorService
{
    public interface IProcessInjecteeExecutor
    {
        Task<uint?> TryExecuteExportedMethod(
            int pid,
            string modulePath,
            SafeHandle injectedModuleHandle,
            string exportedMethodName,
            string argument);

        Task<uint> ExecuteExportedMethod(
            int pid,
            string modulePath,
            SafeHandle injectedModuleHandle,
            string exportedMethodName,
            string argument);
    }
}
