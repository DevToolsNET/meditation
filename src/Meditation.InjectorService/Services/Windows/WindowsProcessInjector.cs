using Meditation.Interop;
using Meditation.Interop.Windows;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Meditation.InjectorService.Services.Windows
{
    internal class WindowsProcessInjector : IProcessInjector
    {
        private readonly ILogger _logger;

        public WindowsProcessInjector(ILogger<WindowsProcessInjector> logger)
        {
            _logger = logger;
        }

        public bool TryInjectModule(int pid, string modulePath, [NotNullWhen(true)] out SafeHandle? remoteModuleHandle)
        {
            // Open target process
            using var processHandle = Kernel32.OpenProcess(ProcessAccessFlags.All, (uint)pid);
            if (processHandle.IsInvalid)
            {
                _logger.LogError("Could not open process with PID = {pid}.", pid);
                remoteModuleHandle = null;
                return false;
            }

            // Allocate memory in target process for module path
            const MemoryProtectionType protection = MemoryProtectionType.ReadWrite;
            var bytesCount = (uint)(modulePath.Length * sizeof(char) + 1);
            using var memoryHandle = Kernel32.VirtualAllocEx(processHandle, bytesCount, protection);
            if (memoryHandle.IsInvalid)
            {
                _logger.LogError("Could not allocate {size} bytes with projection {projection} in process with PID = {pid}.", bytesCount, protection, pid);
                remoteModuleHandle = null;
                return false;
            }

            // Write assembly path to target process
            var modulePathData = Encoding.Unicode.GetBytes(modulePath + '\0');
            if (!Kernel32.WriteProcessMemory(processHandle, memoryHandle, modulePathData, out var written) || written != modulePathData.Length)
            {
                _logger.LogError("Could not write injected module path {path} into allocated memory in process with PID = {pid}.", modulePathData, pid);
                remoteModuleHandle = null;
                return false;
            }

            // Retrieve handle for kernel32 module
            const string kernel32ModuleName = "kernel32";
            using var kernel32ModuleHandle = Kernel32.GetModuleHandle(kernel32ModuleName);
            if (kernel32ModuleHandle.IsInvalid)
            {
                _logger.LogError("Could not obtain handle to module {module}.", kernel32ModuleName);
                remoteModuleHandle = null;
                return false;
            }

            // Retrieve address of LoadLibraryW procedure
            const string loadLibraryWFunctionName = "LoadLibraryW";
            using var loadLibraryProcedureAddress = Kernel32.GetProcAddress(kernel32ModuleHandle, loadLibraryWFunctionName);
            if (loadLibraryProcedureAddress.IsInvalid)
            {
                _logger.LogError("Could not obtain address of function {function} in module {module}.", loadLibraryWFunctionName, kernel32ModuleName);
                remoteModuleHandle = null;
                return false;
            }

            // Create thread in target process and use it to load the module
            using var threadHandle = Kernel32.CreateRemoteThread(processHandle, loadLibraryProcedureAddress, memoryHandle);
            if (threadHandle.IsInvalid)
            {
                _logger.LogError("Could not create remote thread in process with PID = {pid}.", pid);
                remoteModuleHandle = null;
                return false;
            }

            // Wait for the remote thread to finish execution
            if (!Kernel32.WaitForSingleObject(threadHandle, timeout: null))
            {
                _logger.LogError("Could not determine the end of execution for the spawned remote thread in process with PID = {pid}.", pid);
                remoteModuleHandle = null;
                return false;
            }

            // Obtain handle to injected module though thread exit code
            var moduleName = Path.GetFileName(modulePath);
            var module = Process.GetProcessById(pid).Modules.Cast<ProcessModule>().SingleOrDefault(m => m.ModuleName == moduleName);
            if (module == null)
            {
                _logger.LogError("Could not obtain address of injected module {module} in process with PID = {pid}.", moduleName, pid);
                remoteModuleHandle = null;
                return false;
            }

            remoteModuleHandle = new GenericSafeHandle(() => module.BaseAddress, static _ => true, ownsHandle: false);
            return true;
        }
    }
}
