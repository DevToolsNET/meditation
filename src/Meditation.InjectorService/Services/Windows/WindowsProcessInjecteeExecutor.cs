using System;
using Meditation.Interop;
using Meditation.Interop.Windows;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Meditation.InjectorService.Services.Windows
{
    internal class WindowsProcessInjecteeExecutor : IProcessInjecteeExecutor
    {
        public async Task<uint> ExecuteExportedMethod(int pid, string modulePath, SafeHandle injectedModuleHandle, string exportedMethodName, string argument)
        {
            if (await TryExecuteExportedMethod(pid, modulePath, injectedModuleHandle, exportedMethodName, argument) is not { } exitCode)
                throw new Exception($"Could not call exported symbol {exportedMethodName} with argument {argument} in PID = {pid}.");

            return exitCode;
        }

        public Task<uint?> TryExecuteExportedMethod(
            int pid, 
            string modulePath, 
            SafeHandle injectedModuleHandle, 
            string exportedMethodName, 
            string argument)
        {
            if (!TryGetMethodAddressInRemoteProcess(injectedModuleHandle, modulePath, exportedMethodName, out var remoteMethodHandle))
            {
                // Could not get address of method within the injected module in remote process
                // FIXME [#16]: logging
                return Task.FromResult<uint?>(null);
            }

            using var methodHandle = remoteMethodHandle;
            return TryExecuteInRemoteProcess(pid, methodHandle, argument, out var exitCode)
                ? Task.FromResult(exitCode)
                : Task.FromResult<uint?>(null);
        }

        private static bool TryGetMethodAddressInRemoteProcess(
            SafeHandle injectedModuleHandle, 
            string injectedModulePath,
            string exportedMethodName,
            [NotNullWhen(returnValue: true)] out SafeHandle? remoteMethodHandle)
        {
            // Find address of the target method within the remote process memory space
            using var bootstrapModuleHandle = Kernel32.LoadLibraryW(injectedModulePath);
            if (bootstrapModuleHandle.IsInvalid)
            {
                // Could not load injected module
                // FIXME [#16]: logging
                remoteMethodHandle = null;
                return false;
            }
            using var localMethodAddress = Kernel32.GetProcAddress(bootstrapModuleHandle, exportedMethodName);
            if (localMethodAddress.IsInvalid)
            {
                // Could not find exported symbol matching the parameter within the injected module
                // FIXME [#16]: logging
                remoteMethodHandle = null;
                return false;
            }
            var localMethodModuleOffset = localMethodAddress.DangerousGetHandle() - injectedModuleHandle.DangerousGetHandle();
            var remoteMethodAddress = localMethodModuleOffset + injectedModuleHandle.DangerousGetHandle();
            remoteMethodHandle = new GenericSafeHandle(() => remoteMethodAddress, static _ => true, ownsHandle: false);
            return true;
        }

        private static bool TryExecuteInRemoteProcess(
            int pid, 
            SafeHandle remoteMethodHandle,
            string argument,
            [NotNullWhen(returnValue: true)] out uint? returnCode)
        {
            using var remoteProcessHandle = Kernel32.OpenProcess(ProcessAccessFlags.All, (uint)pid);
            if (remoteProcessHandle.IsInvalid)
            {
                // Could not open process
                // FIXME [#16]: logging
                returnCode = null;
                return false;
            }

            // Allocate memory in target process for hook arguments
            const MemoryProtectionType protection = MemoryProtectionType.ReadWrite;
            var bytesCount = (uint)(argument.Length * sizeof(char) + 1);
            using var memoryHandle = Kernel32.VirtualAllocEx(remoteProcessHandle, bytesCount, protection);
            if (memoryHandle.IsInvalid)
            {
                // Unable to allocate memory in target process
                // FIXME [#16]: logging
                returnCode = null;
                return false;
            }

            // Write hook arguments to target process
            var modulePathData = Encoding.Unicode.GetBytes(argument + '\0');
            if (!Kernel32.WriteProcessMemory(remoteProcessHandle, memoryHandle, modulePathData, out var written) || written != modulePathData.Length)
            {
                // Unable to write memory of target process
                // FIXME [#16]: logging
                returnCode = null;
                return false;
            }

            using var remoteThreadHandle = Kernel32.CreateRemoteThread(remoteProcessHandle, remoteMethodHandle, memoryHandle);
            if (remoteThreadHandle.IsInvalid)
            {
                // Could not find exported symbol matching the parameter within the injected module
                // FIXME [#16]: logging
                returnCode = null;
                return false;
            }

            if (!Kernel32.WaitForSingleObject(remoteThreadHandle, timeout: null))
            {
                // Could determine the end of execution in the remote process
                // FIXME [#16]: logging
                returnCode = null;
                return false;
            }

            if (!Kernel32.GetExitCodeThread(remoteThreadHandle, out var exitCode))
            {
                // Could obtain return code from remote process
                // FIXME [#16]: logging
                returnCode = null;
                return false;
            }

            returnCode = exitCode;
            return true;
        }
    }
}
