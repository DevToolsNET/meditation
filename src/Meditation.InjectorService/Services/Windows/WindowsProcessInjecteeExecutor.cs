using System;
using Meditation.Interop;
using Meditation.Interop.Windows;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Meditation.InjectorService.Services.Windows
{
    internal class WindowsProcessInjecteeExecutor : IProcessInjecteeExecutor
    {
        private readonly ILogger _logger;

        public WindowsProcessInjecteeExecutor(ILogger<WindowsProcessInjecteeExecutor> logger)
        {
            _logger = logger;
        }

        public uint ExecuteExportedMethod(int pid, string modulePath, SafeHandle injectedModuleHandle, string exportedMethodName, string argument)
        {
            if (!TryExecuteExportedMethod(pid, modulePath, injectedModuleHandle, exportedMethodName, argument, out var returnCode))
                throw new Exception($"Could not call exported symbol {exportedMethodName} with argument {argument} in PID = {pid}.");

            return returnCode.Value;
        }

        public bool TryExecuteExportedMethod(
            int pid, 
            string modulePath, 
            SafeHandle injectedModuleHandle, 
            string exportedMethodName, 
            string argument,
            [NotNullWhen(returnValue: true)] out uint? returnCode)
        {
            if (!TryGetMethodAddressInRemoteProcess(injectedModuleHandle, modulePath, exportedMethodName, out var remoteMethodHandle))
            {
                _logger.LogError("Could not get address of method {method} within the injected module {module} in remote process with PID = {pid}.", exportedMethodName, modulePath, pid);
                returnCode = null;
                return false;
            }

            using var methodHandle = remoteMethodHandle;
            return TryExecuteInRemoteProcess(pid, methodHandle, argument, out returnCode);
        }

        private bool TryGetMethodAddressInRemoteProcess(
            SafeHandle injectedModuleHandle, 
            string injectedModulePath,
            string exportedMethodName,
            [NotNullWhen(returnValue: true)] out SafeHandle? remoteMethodHandle)
        {
            _logger.LogDebug("Attempting to find address of function {function} within {module}.", exportedMethodName, injectedModulePath);

            // Find address of the target method within the remote process memory space
            using var bootstrapModuleHandle = Kernel32.LoadLibraryW(injectedModulePath);
            if (bootstrapModuleHandle.IsInvalid)
            {
                _logger.LogError("Could not load injected module from path {path} for inspection.", injectedModulePath);
                remoteMethodHandle = null;
                return false;
            }
            using var localMethodAddress = Kernel32.GetProcAddress(bootstrapModuleHandle, exportedMethodName);
            if (localMethodAddress.IsInvalid)
            {
                _logger.LogError("Could not find exported symbol {symbol} within the module {path}.", exportedMethodName, injectedModulePath);
                remoteMethodHandle = null;
                return false;
            }
            var localMethodModuleOffset = localMethodAddress.DangerousGetHandle() - injectedModuleHandle.DangerousGetHandle();
            _logger.LogDebug("Found function {function} within {module} on offset {localMethodModuleOffset:X}.", exportedMethodName, injectedModulePath, localMethodModuleOffset);
            var remoteMethodAddress = localMethodModuleOffset + injectedModuleHandle.DangerousGetHandle();
            _logger.LogDebug("Calculated function {function} within {module} on offset {remoteMethodAddress:X} in remote process.", exportedMethodName, injectedModulePath, remoteMethodAddress);
            remoteMethodHandle = new GenericSafeHandle(() => remoteMethodAddress, static _ => true, ownsHandle: false);
            return true;
        }

        private bool TryExecuteInRemoteProcess(
            int pid, 
            SafeHandle remoteMethodHandle,
            string argument,
            [NotNullWhen(returnValue: true)] out uint? returnCode)
        {
            _logger.LogDebug("Attempting to call function in process with PID = {pid}.", pid);

            using var remoteProcessHandle = Kernel32.OpenProcess(ProcessAccessFlags.All, (uint)pid);
            if (remoteProcessHandle.IsInvalid)
            {
                _logger.LogError("Could not open process with PID = {pid}.", pid);
                returnCode = null;
                return false;
            }

            // Allocate memory in target process for hook arguments
            const MemoryProtectionType protection = MemoryProtectionType.ReadWrite;
            var bytesCount = (uint)(argument.Length * sizeof(char) + 1);
            using var memoryHandle = Kernel32.VirtualAllocEx(remoteProcessHandle, bytesCount, protection);
            if (memoryHandle.IsInvalid)
            {
                _logger.LogError("Could not allocate {size} bytes with projection {projection} in process with PID = {pid}.", bytesCount, protection, pid);
                returnCode = null;
                return false;
            }

            // Write hook arguments to target process
            var modulePathData = Encoding.Unicode.GetBytes(argument + '\0');
            if (!Kernel32.WriteProcessMemory(remoteProcessHandle, memoryHandle, modulePathData, out var written) || written != modulePathData.Length)
            {
                _logger.LogError("Could not write argument {argument} into allocated memory in process with PID = {pid}.", argument, pid);
                returnCode = null;
                return false;
            }

            using var remoteThreadHandle = Kernel32.CreateRemoteThread(remoteProcessHandle, remoteMethodHandle, memoryHandle);
            if (remoteThreadHandle.IsInvalid)
            {
                _logger.LogError("Could not create remote thread in process with PID = {pid}.", pid);
                returnCode = null;
                return false;
            }

            if (!Kernel32.WaitForSingleObject(remoteThreadHandle, timeout: null))
            {
                _logger.LogError("Could not determine the end of execution for the spawned remote thread in process with PID = {pid}.", pid);
                returnCode = null;
                return false;
            }

            if (!Kernel32.GetExitCodeThread(remoteThreadHandle, out var exitCode))
            {
                _logger.LogError("Could not obtain return code from the spawned remote thread in process with PID = {pid}.", pid);
                returnCode = null;
                return false;
            }

            _logger.LogDebug("Received exit code {code:X} from process with PID = {pid}.", exitCode, pid);
            returnCode = exitCode;
            return true;
        }
    }
}
