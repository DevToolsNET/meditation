using System;
using System.Runtime.InteropServices;

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

        public int ExecuteInDefaultAppDomain(
            SafeHandle assemblyPath,
            SafeHandle typeFullName,
            SafeHandle methodName,
            SafeHandle argument, 
            out int returnValue)
        {
            var function = GetNthElementInVirtualMethodTable((int)MethodTableICLRRuntimeHost.ExecuteInDefaultAppDomain);
            return ((delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, out int, int>)(function))(
                Handle,
                assemblyPath.DangerousGetHandle(),
                typeFullName.DangerousGetHandle(),
                methodName.DangerousGetHandle(),
                argument.DangerousGetHandle(),
                out returnValue);
        }
    }
}
