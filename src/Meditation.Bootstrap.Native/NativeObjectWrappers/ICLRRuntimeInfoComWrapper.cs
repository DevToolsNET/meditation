using System;
using System.Diagnostics.CodeAnalysis;

namespace Meditation.Bootstrap.Native.NativeObjectWrappers
{
    internal unsafe class ICLRRuntimeInfoComWrapper : IUnknownComWrapper
    {
        // Virtual method table entries:
        // 0: [IUnknown] QueryInterface
        // 1: [IUnknown] AddRef
        // 2: [IUnknown] Release
        // 3: [ICLRRuntimeInfo] GetVersionString
        // 4: [ICLRRuntimeInfo] GetRuntimeDirectory
        // 5: [ICLRRuntimeInfo] IsLoaded
        // 6: [ICLRRuntimeInfo] LoadErrorString
        // 7: [ICLRRuntimeInfo] LoadLibrary
        // 8: [ICLRRuntimeInfo] GetProcAddress
        // 9: [ICLRRuntimeInfo] GetInterface
        // 10: [ICLRRuntimeInfo] IsLoadable
        // 11: [ICLRRuntimeInfo] SetDefaultStartupFlags
        // 12: [ICLRRuntimeInfo] GetDefaultStartupFlags
        // 13: [ICLRRuntimeInfo] BindAsLegacyV2Runtime
        // 14: [ICLRRuntimeInfo] IsStarted

        public ICLRRuntimeInfoComWrapper(IntPtr handle)
            : base(handle)
        {

        }

        public bool GetInterface<TInterface>(
            Guid rclsid, 
            Guid riid, 
            Func<IntPtr, TInterface> activator, 
            [NotNullWhen(returnValue: true)] out TInterface? ppUnk)
            where TInterface : NativeObjectWrapperBase
        {
            var function = GetNthElementInVirtualMethodTable(9);
            var result = ((delegate* unmanaged<IntPtr, Guid, Guid, out IntPtr, int>)(function))(
                Handle,
                rclsid,
                riid,
                out var rawInterfaceHandle);

            if (result != 0)
            {
                // Could not marshall arguments
                // FIXME [#16]: logging
                ppUnk = null;
                return false;
            }

            ppUnk = activator(rawInterfaceHandle);
            return true;
        }
    }
}
