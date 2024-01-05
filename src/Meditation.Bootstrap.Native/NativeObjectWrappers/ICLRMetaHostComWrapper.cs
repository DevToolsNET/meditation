using Meditation.Bootstrap.Native.Utils;
using System;
using System.Diagnostics.CodeAnalysis;

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

        public bool GetRuntime(
            string pwzVersion, 
            Guid riid, 
            [NotNullWhen(returnValue: true)] out ICLRRuntimeInfoComWrapper? runtimeInfo)
        {
            var function = GetNthElementInVirtualMethodTable((int)MethodTableICLRMetaHost.GetRuntime);
            using var runtimeVersion = MarshalingUtils.ConvertStringToNativeLpcwstr(pwzVersion);
            if (runtimeVersion.IsInvalid)
            {
                // Could not marshall arguments
                // FIXME [#16]: logging
                runtimeInfo = null;
                return false;
            }

            var result = ((delegate* unmanaged<IntPtr, IntPtr, Guid, out IntPtr, int>)(function))(
                Handle,
                runtimeVersion.DangerousGetHandle(),
                riid,
                out var rawRuntimeInfoHandle);

            if (result != 0)
            {
                // Error during method execution
                // FIXME [#16]: logging
                runtimeInfo = null;
                return false;
            }

            runtimeInfo = new ICLRRuntimeInfoComWrapper(rawRuntimeInfoHandle);
            return true;
        }
    }
}
