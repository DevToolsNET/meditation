using Meditation.Bootstrap.Native.NativeObjectWrappers;
using Meditation.Bootstrap.Native.Utils;
using Meditation.Interop;
using Meditation.Interop.Windows;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Meditation.Bootstrap.Native
{
    internal static class NetFrameworkHookingStrategy
    {
        private delegate int CLRCreateInstance(
            [MarshalAs(UnmanagedType.LPStruct), In] Guid clsid,
            [MarshalAs(UnmanagedType.LPStruct), In] Guid riid,
            [Out] out IntPtr ppInterface);

        public static ErrorCode TryInitializeWindowsNetFrameworkProcess(SafeHandle mscoreeModuleHandle, HookArguments args)
        {
            if (!TryObtainClrMetaHost(mscoreeModuleHandle, out var clrMetaHost))
            {
                // Could not obtain instance of ICLRMetaHost
                // FIXME [#16]: logging
                return ErrorCode.NotFound_MetaHost;
            }

            using var metaHost = clrMetaHost;
            if (!TryObtainRuntimeInfo(metaHost, out var info))
            {
                // Could not obtain instance of ICLRRuntimeInfo
                // FIXME [#16]: logging
                return ErrorCode.NotFound_Clr;
            }

            using var runtimeInfo = info;
            if (!TryObtainRuntimeHost(runtimeInfo, out var clrHost))
            {
                // Could not obtain instance of ICLRRuntimeHost
                // FIXME [#16]: logging
                return ErrorCode.NotFound_Host;
            }

            using var runtimeHost = clrHost;
            return CommonHookExecutionStrategy.ExecuteHook(runtimeHost, args);
        }

        /// <summary>
        /// Tries to obtain a handle to ICLRMetaHost. It can be used to obtain information about loaded runtimes.
        /// Operation: https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/clrcreateinstance-function
        /// Result: https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/iclrmetahost-interface
        /// </summary>
        /// <param name="mscoreeModuleHandle">Handle to the mscoree module</param>
        /// <param name="clrMetaHost">(out) COM wrapper for the ICLRMetaHost handle</param>
        /// <returns></returns>
        private static bool TryObtainClrMetaHost(
            SafeHandle mscoreeModuleHandle, 
            [NotNullWhen(returnValue: true)] out ICLRMetaHostComWrapper? clrMetaHost)
        {
            // To query a specific interface/COM object, you need to refer to it by its UUID/CLSID
            // Source: https://github.com/dotnet/runtime/blob/9e31c21bcbb661fc4fa235839a66442a65ef447c/src/coreclr/pal/prebuilt/inc/metahost.h#L99
            var clsidClrMetaHost = new Guid(0x9280188d, 0xe8e, 0x4867, 0xb3, 0xc, 0x7f, 0xa8, 0x38, 0x84, 0xe8, 0xde);

            // Source: https://github.com/dotnet/runtime/blob/9e31c21bcbb661fc4fa235839a66442a65ef447c/src/coreclr/pal/prebuilt/inc/metahost.h#L98
            var iidIClrMetaHost = new Guid(0xD332DB9E, 0xB9B3, 0x4125, 0x82, 0x07, 0xA1, 0x48, 0x84, 0xF5, 0x32, 0x16);

            // In order to obtain the handle for ICLRMetaHost, we must call a DLLEXPORT ("CLRCreateInstance" - it is part of the public API)
            // The exported symbol can be found in the main mscoree module. Therefore, we can use the GetProcAddress to obtain the address of the desired function
            var getClrMetaHostFunctionHandle = Kernel32.GetProcAddress(mscoreeModuleHandle, nameof(CLRCreateInstance));

            // Execute CLRCreateInstance
            var clrMetaHostProvider = Marshal.GetDelegateForFunctionPointer<CLRCreateInstance>(getClrMetaHostFunctionHandle.DangerousGetHandle());
            var result = clrMetaHostProvider(clsidClrMetaHost, iidIClrMetaHost, out var rawClrMetaHostHandle);

            if (result != 0)
            {
                // Could not obtain ICLRMetaHost
                // FIXME [#16]: log return value
                clrMetaHost = null;
                return false;
            }

            clrMetaHost = new ICLRMetaHostComWrapper(rawClrMetaHostHandle);
            return true;
        }

        /// <summary>
        /// Tries to obtain a handle to ICLRRuntimeInfo. It can be used to obtain handle to its host.
        /// Operation: https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/iclrmetahost-getruntime-method
        /// Result: https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/iclrruntimeinfo-interface
        /// </summary>
        /// <param name="clrMetaHost">COM wrapper for the ICLRMetaHost handle</param>
        /// <param name="runtimeInfo">(out) COM wrapper for the ICLRRuntimeInfo handle</param>
        /// <returns>True if succeeded, false otherwise</returns>
        private static bool TryObtainRuntimeInfo(
            ICLRMetaHostComWrapper clrMetaHost,
            [NotNullWhen(returnValue: true)] out ICLRRuntimeInfoComWrapper? runtimeInfo)
        {
            // Since .NET Framework 4.0, the CLR version has been in the following format: v4.0.30319.xxxxx
            // We need to specify only the first 3 segments of the version, all previous versions are out of support anyway
            const string clrRuntimeVersionV4 = "v4.0.30319";

            // To query a specific interface/COM object, you need to refer to it by its UUID/CLSID
            // Source: https://github.com/dotnet/runtime/blob/9e31c21bcbb661fc4fa235839a66442a65ef447c/src/coreclr/inc/metahost.idl#L55
            var iidIClrRuntimeInfo = new Guid(0xBD39D1D2, 0xBA2F, 0x486a, 0x89, 0xB0, 0xB4, 0xB0, 0xCB, 0x46, 0x68, 0x91);

            // Marshall arguments
            using var runtimeVersion = MarshalingUtils.ConvertStringToNativeLpcwstr(clrRuntimeVersionV4);
            if (runtimeVersion.IsInvalid)
            {
                // Could not marshall arguments
                // FIXME [#16]: logging
                runtimeInfo = null;
                return false;
            }

            // Try obtain runtime info
            var result = clrMetaHost.GetRuntime(runtimeVersion, iidIClrRuntimeInfo, out runtimeInfo);
            if (result != 0)
            {
                // Could not obtain instance of ICLRRuntimeInfo
                // FIXME [#16]: log return value
                runtimeInfo = null;
            }

            return result == 0;
        }

        /// <summary>
        /// Tries to obtain handle to the ICLRRuntimeHost interface. We can use it to execute arbitrary managed code from a native module (self).
        /// Operation: https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/iclrruntimeinfo-getinterface-method
        /// Result: https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/iclrruntimehost-interface
        /// </summary>
        /// <param name="runtimeInfo">COM wrapper for the ICLRRuntimeInfo handle</param>
        /// <param name="runtimeHost">(out) COM wrapper for the ICLRRuntimeHost handle</param>
        /// <returns>True if succeeded, false otherwise</returns>
        private static bool TryObtainRuntimeHost(
            ICLRRuntimeInfoComWrapper runtimeInfo,
            [NotNullWhen(returnValue: true)] out ICLRRuntimeHostComWrapper? runtimeHost)
        {
            // To query a specific interface/COM object, you need to refer to it by its UUID/CLSID
            // Source: https://github.com/microsoft/win32metadata/blob/1ba2527a59da3f1c1b4809bb053d03f300af3c09/generation/WinSDK/RecompiledIdlHeaders/um/mscoree.h#L530
            var clsidClrRuntimeHost = new Guid(0x90F1A06E, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);

            // Source: https://github.com/dotnet/runtime/blob/9e31c21bcbb661fc4fa235839a66442a65ef447c/src/coreclr/inc/mscoree.idl#L27
            var iidIClrRuntimeHost = new Guid(0x90F1A06C, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);

            // Try obtain runtime host
            var result = runtimeInfo.GetInterface(clsidClrRuntimeHost, iidIClrRuntimeHost, ptr => new ICLRRuntimeHostComWrapper(ptr), out runtimeHost);
            if (result != 0)
            {
                // Could not obtain instance of ICLRRuntimeHost
                // FIXME [#16]: logging
                runtimeHost = null;
            }

            return result == 0;
        }
    }
}
