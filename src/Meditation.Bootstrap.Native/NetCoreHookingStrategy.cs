using Meditation.Interop;
using Meditation.Interop.Windows;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Meditation.Bootstrap.Native
{
    internal static unsafe class NetCoreHookingStrategy
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

            if (!TryExecuteInDefaultAppDomain(clrRuntimeHost, args, out var result))
            {
                // Error during execution of hook's managed entrypoint
                // FIXME [#16]: logging
                return ErrorCode.RuntimeError;
            }

            return (result.Value == 0) ? ErrorCode.Ok : ErrorCode.RuntimeError;
        }

        private static bool TryObtainClrRuntimeHostHandle(SafeHandle coreClrModuleHandle, out IntPtr clrRuntimeHostHandle)
        {
            var clrRuntimeHostId = new Guid("90F1A06C-7712-4762-86B5-7A5EBA6BDB02");
            var getClrRuntimeHostFunctionHandle = Kernel32.GetProcAddress(coreClrModuleHandle, nameof(GetCLRRuntimeHost));
            var clrRuntimeHostProvider = Marshal.GetDelegateForFunctionPointer<GetCLRRuntimeHost>(getClrRuntimeHostFunctionHandle.DangerousGetHandle());
            return clrRuntimeHostProvider(clrRuntimeHostId, out clrRuntimeHostHandle) == 0;
        }

        private static bool TryExecuteInDefaultAppDomain(IntPtr clrRuntimeHost, HookArguments args, [NotNullWhen(returnValue: true)] out int? returnValue)
        {
            // TODO: Simplify interop once COM source generators are available: https://github.com/dotnet/runtime/issues/66674
            // This is a public API defined in mscoree.idl
            // See for reference: https://github.com/dotnet/runtime/blob/main/src/coreclr/inc/mscoree.idl
            // COM is currently not supported in NativeAOT: https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/docs/limitations.md
            // We can call the member function directly by abusing the fact how these objects are represented in memory

            // COM ABI requires that pointer to an object is the pointer to type's vtable
            var instance = clrRuntimeHost;
            var vtable = *(void***)instance;
            // Entries in vtable:
            // 0: [IUnknown] QueryInterface
            // 1: [IUnknown] AddRef
            // 2: [IUnknown] Release
            // 3: [ICLRRuntimeHost] Start
            // 4: [ICLRRuntimeHost] Stop
            // 5: [ICLRRuntimeHost] SetHostControl
            // 6: [ICLRRuntimeHost] GetCLRControl
            // 7: [ICLRRuntimeHost] UnloadAppDomain
            // 8: [ICLRRuntimeHost] ExecuteInAppDomain
            // 9: [ICLRRuntimeHost] GetCurrentAppDomainId
            // 10: [ICLRRuntimeHost] ExecuteApplication
            // 11: [ICLRRuntimeHost] ExecuteInDefaultAppDomain

            // ExecuteInDefaultAppDomain is in the index 11 of vtable
            var function = *(vtable + 11);

            // Function expects strings to be passed as LPCWSTR (wchar_t instead of default marshaling to char)
            using var assemblyPathNativeStringHandle = ConvertStringToNativeLpcwstr(args.AssemblyPath);
            using var typeNameNativeStringHandle = ConvertStringToNativeLpcwstr(args.TypeFullName);
            using var methodNameNativeStringHandle = ConvertStringToNativeLpcwstr(args.MethodName);
            using var argumentNativeStringHandle = ConvertStringToNativeLpcwstr(args.Argument);
            if (argumentNativeStringHandle.IsInvalid ||
                typeNameNativeStringHandle.IsInvalid ||
                methodNameNativeStringHandle.IsInvalid ||
                argumentNativeStringHandle.IsInvalid)
            {
                // Could not marshall arguments
                // FIXME [#16]: logging
                returnValue = null;
                return false;
            }

            var result =  ((delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, out int, int>)(function))(
                instance,
                assemblyPathNativeStringHandle.DangerousGetHandle(),
                typeNameNativeStringHandle.DangerousGetHandle(),
                methodNameNativeStringHandle.DangerousGetHandle(),
                argumentNativeStringHandle.DangerousGetHandle(),
                out var exitCode);

            if (result != 0)
            {
                // Error during method execution
                // FIXME [#16]: logging
                returnValue = result;
                return false;
            }

            returnValue = exitCode;
            return true;
        }

        private static SafeHandle ConvertStringToNativeLpcwstr(string input)
        {
            return new GenericSafeHandle(
                acquireDelegate: () => Marshal.StringToHGlobalUni(input), 
                releaseDelegate: ptr => 
                {
                    Marshal.FreeHGlobal(ptr);
                    return true;
                });
        }
    }
}
