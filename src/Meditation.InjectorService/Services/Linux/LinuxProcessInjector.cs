using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Meditation.Interop;

namespace Meditation.InjectorService.Services.Linux
{
    internal class LinuxProcessInjector : IProcessInjector
    {
        public async Task<SafeHandle> TryInjectModule(int pid, string modulePath)
        {
            if (!await Gdb.IsInstalled())
            {
                // Could not find usable gdb
                // FIXME [#16]: logging
                return GenericSafeHandle.Invalid;
            }

            var handle = await Gdb.TryInjectModule(pid, modulePath);
            if (handle.IsInvalid)
            {
                // Could not inject module
                // FIXME [#16]: logging
                return GenericSafeHandle.Invalid;
            }

            return handle;
        }
    }
}
