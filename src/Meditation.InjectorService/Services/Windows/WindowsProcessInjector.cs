using Meditation.Interop;
using Meditation.Interop.Windows;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Meditation.InjectorService.Services.Windows
{
    internal class WindowsProcessInjector : IProcessInjector
    {
        public bool TryInjectModuleToProcess(int pid, string modulePath, [NotNullWhen(true)] out SafeHandle? remoteModuleHandle)
        {
            // Open target process
            using var processHandle = Kernel32.OpenProcess(ProcessAccessFlags.All, (uint)pid);
            if (processHandle.IsInvalid)
            {
                // Could not open process
                // FIXME [#16]: logging
                remoteModuleHandle = null;
                return false;
            }

            // Allocate memory in target process for module path
            const AllocationType allocation = AllocationType.Commit;
            const MemoryProtection protection = MemoryProtection.ReadWrite;
            var bytesCount = (uint)(modulePath.Length * sizeof(char) + 1);
            using var memoryHandle = Kernel32.VirtualAllocEx(processHandle, bytesCount, allocation, protection);
            if (memoryHandle.IsInvalid)
            {
                // Unable to allocate memory in target process
                // FIXME [#16]: logging
                remoteModuleHandle = null;
                return false;
            }

            // Write assembly path to target process
            var modulePathData = Encoding.Unicode.GetBytes(modulePath + '\0');
            if (!Kernel32.WriteProcessMemory(processHandle, memoryHandle, modulePathData, out var written) || written != modulePathData.Length)
            {
                // Unable to write memory of target process
                // FIXME [#16]: logging
                remoteModuleHandle = null;
                return false;
            }

            // Retrieve handle for kernel32 module
            using var kernel32ModuleHandle = Kernel32.GetModuleHandle("kernel32");
            if (kernel32ModuleHandle.IsInvalid)
            {
                // Unable to obtain handle to kernel32
                // FIXME [#16]: logging
                remoteModuleHandle = null;
                return false;
            }

            // Retrieve address of LoadLibraryW procedure
            using var loadLibraryProcedureAddress = Kernel32.GetProcAddress(kernel32ModuleHandle, "LoadLibraryW");
            if (loadLibraryProcedureAddress.IsInvalid)
            {
                // Unable to obtain address of LoadLibraryW procedure
                // FIXME [#16]: logging
                remoteModuleHandle = null;
                return false;
            }

            // Create thread in target process and use it to load the module
            using var threadHandle = Kernel32.CreateRemoteThread(processHandle, loadLibraryProcedureAddress, memoryHandle);
            if (threadHandle.IsInvalid)
            {
                // Could not create thread in target process
                // FIXME [#16]: logging
                remoteModuleHandle = null;
                return false;
            }

            // Wait for the remote thread to finish execution
            if (!Kernel32.WaitForSingleObject(threadHandle, timeout: null))
            {
                // Error while waiting for remote thread to finish
                // FIXME [#16]: logging
                remoteModuleHandle = null;
                return false;
            }

            // Obtain handle to injected module though thread exit code
            if (!Kernel32.GetExitCodeThread(threadHandle, out var moduleHandle))
            {
                // Error while obtaining thread's exit code
                // FIXME: logging
                remoteModuleHandle = null;
                return false;
            }

            // Cleanup
            remoteModuleHandle = new GenericSafeHandle(() => moduleHandle, static _ => true, ownsHandle: false);
            return true;
        }

        public bool TryExecuteStaticMethodInDefaultProcessAppDomain(int pid, string assemblyPath, string fullTypeName, string methodName, string arg, out int returnCode)
        {
            throw new NotImplementedException();
        }
    }
}
