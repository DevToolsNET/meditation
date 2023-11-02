using Meditation.Bootstrap.Native.NativeObjectWrappers;
using Meditation.Interop;
using Meditation.Interop.Windows;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Meditation.Bootstrap.Native
{
    internal static unsafe class NetFrameworkHookingStrategy
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
                return ErrorCode.MetaHostNotFound;
            }

            using var metaHost = clrMetaHost;
            if (!TryObtainRuntimeInfo(metaHost, out var info))
            {
                // Could not obtain instance of ICLRRuntimeInfo
                // FIXME [#16]: logging
                return ErrorCode.ClrNotFound;
            }

            using var runtimeInfo = info;
            if (!TryObtainRuntimeHost(runtimeInfo, out var clrHost))
            {
                // Could not obtain instance of ICLRRuntimeHost
                // FIXME [#16]: logging
                return ErrorCode.HostNotFound;
            }

            using var runtimeHost = clrHost;
            if (!runtimeHost.ExecuteInDefaultAppDomain(args.AssemblyPath, args.TypeFullName, args.MethodName, args.Argument, out var result))
            {
                // Error during execution of hook's managed entrypoint
                // FIXME [#16]: logging
                return ErrorCode.RuntimeError;
            }

            return (result.Value == 0) ? ErrorCode.Ok : ErrorCode.RuntimeError;
        }

        private static bool TryObtainClrMetaHost(
            SafeHandle mscoreeModuleHandle, 
            [NotNullWhen(returnValue: true)] out ICLRMetaHostComWrapper? clrMetaHost)
        {
            // Source: https://github.com/dotnet/runtime/blob/main/src/coreclr/pal/prebuilt/inc/metahost.h
            var clsidClrMetaHost = new Guid(0x9280188d, 0xe8e, 0x4867, 0xb3, 0xc, 0x7f, 0xa8, 0x38, 0x84, 0xe8, 0xde);
            var iidIClrMetaHost = new Guid(0xD332DB9E, 0xB9B3, 0x4125, 0x82, 0x07, 0xA1, 0x48, 0x84, 0xF5, 0x32, 0x16);

            // Try obtain instance of ICLRMetaHost
            var getClrMetaHostFunctionHandle = Kernel32.GetProcAddress(mscoreeModuleHandle, nameof(CLRCreateInstance));
            var clrMetaHostProvider = Marshal.GetDelegateForFunctionPointer<CLRCreateInstance>(getClrMetaHostFunctionHandle.DangerousGetHandle());
            if (clrMetaHostProvider(clsidClrMetaHost, iidIClrMetaHost, out var rawClrMetaHostHandle) != 0)
            {
                clrMetaHost = null;
                return false;
            }

            clrMetaHost = new ICLRMetaHostComWrapper(rawClrMetaHostHandle);
            return true;
        }

        private static bool TryObtainRuntimeInfo(
            ICLRMetaHostComWrapper clrMetaHost,
            [NotNullWhen(returnValue: true)] out ICLRRuntimeInfoComWrapper? runtimeInfo)
        {
            const string clrRuntimeVersionV4 = "v4.0.30319";
            var iidIClrRuntimeInfo = new Guid(0xBD39D1D2, 0xBA2F, 0x486a, 0x89, 0xB0, 0xB4, 0xB0, 0xCB, 0x46, 0x68, 0x91);
            return clrMetaHost.GetRuntime(clrRuntimeVersionV4, iidIClrRuntimeInfo, out runtimeInfo);
        }

        private static bool TryObtainRuntimeHost(
            ICLRRuntimeInfoComWrapper runtimeInfo,
            [NotNullWhen(returnValue: true)] out ICLRRuntimeHostComWrapper? runtimeHost)
        {
            var clsidClrRuntimeHost = new Guid(0x90F1A06E, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02 );
            var iidIClrRuntimeHost = new Guid(0x90F1A06C, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);
            return runtimeInfo.GetInterface(clsidClrRuntimeHost, iidIClrRuntimeHost, ptr => new ICLRRuntimeHostComWrapper(ptr), out runtimeHost);
        }
    }
}
