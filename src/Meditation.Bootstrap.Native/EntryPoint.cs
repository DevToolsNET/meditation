using Meditation.Interop;
using Meditation.Interop.Windows;
using System;
using System.Runtime.InteropServices;

namespace Meditation.Bootstrap.Native
{
    public static class EntryPoint
    {
        /// <summary>
        /// This method exists only for testing purposes. Its aim is to validate that native module injection logic works.
        /// Note(invocations): this method will be executed dynamically by spawning a native thread in the target process and running the unmanaged export (used by unit tests).
        /// Note(signature): all native exports need to have a concrete signature. Static method, System.UInt32 as return value, System.IntPtr as its only parameter.
        /// Note(return value): unit tests expect a specific constant to be returned (0xABCD_EF98), anything else will be treated as an injection failure.
        /// References: for more details on these restrictions, see this: https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createremotethread
        /// </summary>
        /// <param name="_">Ignored</param>
        /// <returns>0xABCD_EF98</returns>
        [UnmanagedCallersOnly(EntryPoint = "MeditationSanityCheck")]
        public static NativeHookErrorCode SanityCheck(IntPtr _)
        {
            return (NativeHookErrorCode)0xABCD_EF98;
        }

        /// <summary>
        /// This method lays initial works to setup a hooking environment. As a last step, it will call into managed code (e.g. Meditation.Bootstrap.Managed) to perform the actual hook.
        /// Note(invocations): this method will be executed dynamically by spawning a native thread in the target process and running the unmanaged export (used by Meditation.InjectorService).
        /// Note(signature): all native exports need to have a concrete signature. Static method, System.UInt32 as return value, System.IntPtr as its only parameter.
        /// Note(return value): return 0 to indicate that the hook was successful. Use different values to indicate failures. These values will be interpreted by Meditation's injection service.
        /// References: for more details on these restrictions, see this: https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createremotethread
        /// </summary>
        /// <param name="nativeWideStringHookArgs">String input marshaled to LPCWSTR. This specifies what managed code to execute after environment initialization (format: assemblyPath#typeFullName#methodName#argument)</param>
        /// <returns>Zero on success, other values on failures</returns>
        [UnmanagedCallersOnly(EntryPoint = "MeditationInitialize")]
        public static NativeHookErrorCode NativeEntryPoint(IntPtr nativeWideStringHookArgs)
        {
            try
            {
                if (!NativeHookArguments.TryParse(nativeWideStringHookArgs, out var errorCode, out var hookArguments))
                    return errorCode;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return NativeEntryPointWindows(hookArguments);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return NativeHookErrorCode.NotImplemented_OperatingSystem;

                return NativeHookErrorCode.NotSupported_OperatingSystem;
            }
            catch (Exception)
            {
                // Unhandled exception during hooking
                // FIXME [#16]: logging
                return NativeHookErrorCode.InternalError;
            }
        }

        private static NativeHookErrorCode NativeEntryPointWindows(NativeHookArguments arguments)
        {
            const string coreClrModule = "coreclr.dll";
            const string mscoreeModule = "mscoree.dll";

            // Test for .NET Core application
            using var coreClrModuleHandle = Kernel32.GetModuleHandle(coreClrModule);
            if (!coreClrModuleHandle.IsInvalid)
                return NetCoreHookingStrategy.TryInitializeWindowsNetCoreProcess(coreClrModuleHandle, arguments);

            // Test for .NET Framework application
            using var mscoreeModuleHandle = Kernel32.GetModuleHandle(mscoreeModule);
            if (!mscoreeModuleHandle.IsInvalid)
                return NetFrameworkHookingStrategy.TryInitializeWindowsNetFrameworkProcess(mscoreeModuleHandle, arguments);

            // Attempt to inject an unsupported process
            return NativeHookErrorCode.NotSupported_Process;
        }
    }
}