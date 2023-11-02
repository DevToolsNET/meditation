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

        public static ErrorCode TryInitializeWindowsNetCoreProcess(SafeHandle coreClrModuleHandle, HookArguments args)
        {
            if (!TryObtainClrRuntimeHostHandle(coreClrModuleHandle, out var clrRuntimeHost))
            {
                // Could not obtain instance of the CoreCLR Runtime Host
                // FIXME [#16]: logging
                return ErrorCode.HostNotFound;
            }

            using var runtimeHost = clrRuntimeHost;
            if (!runtimeHost.ExecuteInDefaultAppDomain(args.AssemblyPath, args.TypeFullName, args.MethodName, args.Argument, out var result))
            {
                // Error during execution of hook's managed entrypoint
                // FIXME [#16]: logging
                return ErrorCode.RuntimeError;
            }

            return (result.Value == 0) ? ErrorCode.Ok : ErrorCode.RuntimeError;
        }

        private static bool TryObtainClrRuntimeHostHandle(
            SafeHandle coreClrModuleHandle, 
            [NotNullWhen(returnValue: true)] out ICLRRuntimeHostComWrapper? clrRuntimeHostHandle)
        {
            var clrRuntimeHostId = new Guid(0x90F1A06C, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);
            var getClrRuntimeHostFunctionHandle = Kernel32.GetProcAddress(coreClrModuleHandle, nameof(GetCLRRuntimeHost));
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
