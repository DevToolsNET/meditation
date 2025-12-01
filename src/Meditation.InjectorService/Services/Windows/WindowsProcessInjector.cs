using Meditation.Interop;
using Meditation.Interop.Windows;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Meditation.InjectorService.Services.Windows
{
    internal class WindowsProcessInjector : IProcessInjector
    {
        public Task<SafeHandle> TryInjectModule(int pid, string modulePath)
        {
            // Open target process
            using var processHandle = Kernel32.OpenProcess(ProcessAccessFlags.All, (uint)pid);
            if (processHandle.IsInvalid)
            {
                // Could not open process
                // FIXME [#16]: logging
                return Task.FromResult<SafeHandle>(GenericSafeHandle.Invalid);
            }

            // Allocate memory in target process for module path
            const MemoryProtectionType protection = MemoryProtectionType.ReadWrite;
            var bytesCount = (uint)(modulePath.Length * sizeof(char) + 1);
            using var memoryHandle = Kernel32.VirtualAllocEx(processHandle, bytesCount, protection);
            if (memoryHandle.IsInvalid)
            {
                // Unable to allocate memory in target process
                // FIXME [#16]: logging
                return Task.FromResult<SafeHandle>(GenericSafeHandle.Invalid);
            }

            // Write assembly path to target process
            var modulePathData = Encoding.Unicode.GetBytes(modulePath + '\0');
            if (!Kernel32.WriteProcessMemory(processHandle, memoryHandle, modulePathData, out var written) || written != modulePathData.Length)
            {
                // Unable to write memory of target process
                // FIXME [#16]: logging
                return Task.FromResult<SafeHandle>(GenericSafeHandle.Invalid);
            }

            // Retrieve handle for kernel32 module
            using var kernel32ModuleHandle = Kernel32.GetModuleHandle("kernel32");
            if (kernel32ModuleHandle.IsInvalid)
            {
                // Unable to obtain handle to kernel32
                // FIXME [#16]: logging
                return Task.FromResult<SafeHandle>(GenericSafeHandle.Invalid);
            }

            // Retrieve address of LoadLibraryW procedure
            using var loadLibraryProcedureAddress = Kernel32.GetProcAddress(kernel32ModuleHandle, "LoadLibraryW");
            if (loadLibraryProcedureAddress.IsInvalid)
            {
                // Unable to obtain address of LoadLibraryW procedure
                // FIXME [#16]: logging
                return Task.FromResult<SafeHandle>(GenericSafeHandle.Invalid);
            }

            // Create thread in target process and use it to load the module
            using var threadHandle = Kernel32.CreateRemoteThread(processHandle, loadLibraryProcedureAddress, memoryHandle);
            if (threadHandle.IsInvalid)
            {
                // Could not create thread in target process
                // FIXME [#16]: logging
                return Task.FromResult<SafeHandle>(GenericSafeHandle.Invalid);
            }

            // Wait for the remote thread to finish execution
            if (!Kernel32.WaitForSingleObject(threadHandle, timeout: null))
            {
                // Error while waiting for remote thread to finish
                // FIXME [#16]: logging
                return Task.FromResult<SafeHandle>(GenericSafeHandle.Invalid);
            }

            // Obtain handle to injected module though thread exit code
            var moduleName = Path.GetFileName(modulePath);
            var module = Process.GetProcessById(pid).Modules.Cast<ProcessModule>().SingleOrDefault(m => m.ModuleName == moduleName);
            if (module == null)
            {
                // Error while obtaining base address for loaded module
                // FIXME [#16]: logging
                return Task.FromResult<SafeHandle>(GenericSafeHandle.Invalid);
            }

            return Task.FromResult<SafeHandle>(new GenericSafeHandle(() => module.BaseAddress, static _ => true, ownsHandle: false));
        }
    }
}
