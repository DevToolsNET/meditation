using Meditation.Bootstrap.Native.NativeObjectWrappers;
using Meditation.Interop;
using Meditation.Interop.Windows;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Meditation.Bootstrap.Native
{
    internal static class NetCoreHookingStrategy
    {
        private delegate int GetCLRRuntimeHost([MarshalAs(UnmanagedType.LPStruct), In] Guid riid, [Out] out IntPtr ppUnk);

        public static NativeHookErrorCode TryInitializeWindowsNetCoreProcess(SafeHandle coreClrModuleHandle, NativeHookArguments args)
        {
            if (!TryObtainClrRuntimeHostHandle(coreClrModuleHandle, out var clrRuntimeHost))
            {
                // Could not obtain instance of the CoreCLR Runtime Host
                // FIXME [#16]: logging
                return NativeHookErrorCode.NotFound_Host;
            }

            using var runtimeHost = clrRuntimeHost;
            return CommonHookExecutionStrategy.ExecuteHook(runtimeHost, args);
        }

        /// <summary>
        /// Tries to obtain handle to the ICLRRuntimeHost interface. We can use it to execute arbitrary managed code from a native module (self).
        /// Reference(interface): https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/iclrruntimehost-interface
        /// Reference(desired method): https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/iclrruntimehost-executeindefaultappdomain-method
        /// </summary>
        /// <param name="coreClrModuleHandle">Handle to the coreclr module</param>
        /// <param name="clrRuntimeHostHandle">(out) Handle to the ICLRRuntimeHost interface</param>
        /// <returns>True on success, false otherwise</returns>
        private static bool TryObtainClrRuntimeHostHandle(
            SafeHandle coreClrModuleHandle, 
            [NotNullWhen(returnValue: true)] out ICLRRuntimeHostComWrapper? clrRuntimeHostHandle)
        {
            // To query a specific interface/COM object, you need to refer to it by its UUID/CLSID
            // Source: https://github.com/dotnet/runtime/blob/9e31c21bcbb661fc4fa235839a66442a65ef447c/src/coreclr/inc/mscoree.idl#L27
            var clrRuntimeHostId = new Guid(0x90F1A06C, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);

            // In order to obtain the handle for CLRRuntimeHost, we must call a DLLEXPORT ("GetCLRRuntimeHost" - it is part of the public API)
            // The definition of this specific export can be found here: https://github.com/dotnet/runtime/blob/9e31c21bcbb661fc4fa235839a66442a65ef447c/src/coreclr/vm/corhost.cpp#L827
            // The exported symbol can be found in the main coreclr module. Therefore, we can use the GetProcAddress to obtain the address of the desired function
            var getClrRuntimeHostFunctionHandle = Kernel32.GetProcAddress(coreClrModuleHandle, nameof(GetCLRRuntimeHost));

            // Execute GetCLRRuntimeHost
            var clrRuntimeHostProvider = Marshal.GetDelegateForFunctionPointer<GetCLRRuntimeHost>(getClrRuntimeHostFunctionHandle.DangerousGetHandle());
            if (clrRuntimeHostProvider(clrRuntimeHostId, out var rawClrRuntimeHostHandle) != 0)
            {
                clrRuntimeHostHandle = null;
                return false;
            }

            clrRuntimeHostHandle = new ICLRRuntimeHostComWrapper(rawClrRuntimeHostHandle);
            return true;
        }
    }
}
