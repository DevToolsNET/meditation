using System;
using System.Diagnostics.CodeAnalysis;
using Meditation.Bootstrap.Native.Utils;

namespace Meditation.Bootstrap.Native.NativeObjectWrappers
{
    internal unsafe class ICLRRuntimeHostComWrapper : IUnknownComWrapper
    {
        // Virtual method table entries:
        enum MethodTableICLRRuntimeHost
        {
            // [IUnknown]
            QueryInterface = 0,
            // [IUnknown]
            AddRef = 1,
            // [IUnknown]
            Release = 2,
            // [ICLRRuntimeHost]
            Start = 3,
            // [ICLRRuntimeHost]
            Stop = 4,
            // [ICLRRuntimeHost]
            SetHostControl = 5,
            // [ICLRRuntimeHost]
            GetCLRControl = 6,
            // [ICLRRuntimeHost]
            UnloadAppDomain = 7,
            // [ICLRRuntimeHost]
            ExecuteInAppDomain = 8,
            // [ICLRRuntimeHost]
            GetCurrentAppDomainId = 9,
            // [ICLRRuntimeHost]
            ExecuteApplication = 10,
            // [ICLRRuntimeHost]
            ExecuteInDefaultAppDomain = 11,
        }

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
            var function = GetNthElementInVirtualMethodTable((int)MethodTableICLRRuntimeHost.ExecuteInDefaultAppDomain);
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
