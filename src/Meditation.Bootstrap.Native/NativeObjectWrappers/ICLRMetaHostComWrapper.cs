using Meditation.Bootstrap.Native.Utils;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Meditation.Bootstrap.Native.NativeObjectWrappers
{
    internal unsafe class ICLRMetaHostComWrapper : IUnknownComWrapper
    {
        // Virtual method table entries:
        // 0: [IUnknown] QueryInterface
        // 1: [IUnknown] AddRef
        // 2: [IUnknown] Release
        // 3: [ICLRMetaHost] GetRuntime
        // 4: [ICLRMetaHost] GetVersionFromFile
        // 5: [ICLRMetaHost] EnumerateInstalledRuntimes
        // 6: [ICLRMetaHost] EnumerateLoadedRuntimes
        // 7: [ICLRMetaHost] RequestRuntimeLoadedNotification
        // 8: [ICLRMetaHost] QueryLegacyV2RuntimeBinding
        // 9: [ICLRMetaHost] ExitProcess

        public ICLRMetaHostComWrapper(IntPtr handle)
            : base(handle)
        {

        }

        public bool GetRuntime(
            string pwzVersion, 
            Guid riid, 
            [NotNullWhen(returnValue: true)] out ICLRRuntimeInfoComWrapper? runtimeInfo)
        {
            var function = GetNthElementInVirtualMethodTable(3);
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
