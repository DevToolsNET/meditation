using System;
using System.Runtime.InteropServices;

namespace Meditation.Bootstrap.Native.NativeObjectWrappers
{
    internal unsafe class ICLRMetaHostComWrapper : IUnknownComWrapper
    {
        // Virtual method table entries:
        enum MethodTableICLRMetaHost
        {
            // [IUnknown]
            QueryInterface = 0,
            // [IUnknown]
            AddRef = 1,
            // [IUnknown]
            Release = 2,
            // [ICLRMetaHost]
            GetRuntime = 3,
            // [ICLRMetaHost]
            GetVersionFromFile = 4,
            // [ICLRMetaHost]
            EnumerateInstalledRuntimes = 5,
            // [ICLRMetaHost]
            EnumerateLoadedRuntimes = 6,
            // [ICLRMetaHost]
            RequestRuntimeLoadedNotification = 7,
            // [ICLRMetaHost]
            QueryLegacyV2RuntimeBinding = 8,
            // [ICLRMetaHost]
            ExitProcess = 9
        }

        public ICLRMetaHostComWrapper(IntPtr handle)
            : base(handle)
        {

        }

        public int GetRuntime(
            SafeHandle pwzVersion, 
            Guid riid, 
            out ICLRRuntimeInfoComWrapper? runtimeInfo)
        {
            var function = GetNthElementInVirtualMethodTable((int)MethodTableICLRMetaHost.GetRuntime);
            var result = ((delegate* unmanaged<IntPtr, IntPtr, Guid, out IntPtr, int>)(function))(
                Handle,
                pwzVersion.DangerousGetHandle(),
                riid,
                out var rawRuntimeInfoHandle);

            runtimeInfo = result == 0 ? new ICLRRuntimeInfoComWrapper(rawRuntimeInfoHandle) : null;
            return result;
        }
    }
}
