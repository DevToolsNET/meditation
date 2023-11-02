using System;
using System.Diagnostics.CodeAnalysis;
using Meditation.Bootstrap.Native.Utils;

namespace Meditation.Bootstrap.Native.NativeObjectWrappers
{
    internal unsafe class ICLRRuntimeHostComWrapper : IUnknownComWrapper
    {
        // Virtual method table entries:
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

        public ICLRRuntimeHostComWrapper(IntPtr handle)
            : base(handle)
        {

        }

        public bool ExecuteInDefaultAppDomain(
            string assemblyPath, 
            string typeFullName, 
            string methodName, 
            string argument, 
            [NotNullWhen(returnValue: true)] out int? returnValue)
        {
            var function = GetNthElementInVirtualMethodTable(11);
            using var assemblyPathNativeStringHandle = MarshalingUtils.ConvertStringToNativeLpcwstr(assemblyPath);
            using var typeNameNativeStringHandle = MarshalingUtils.ConvertStringToNativeLpcwstr(typeFullName);
            using var methodNameNativeStringHandle = MarshalingUtils.ConvertStringToNativeLpcwstr(methodName);
            using var argumentNativeStringHandle = MarshalingUtils.ConvertStringToNativeLpcwstr(argument);
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

            var result = ((delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, out int, int>)(function))(
                Handle,
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
    }
}
