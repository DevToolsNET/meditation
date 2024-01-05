using System;
using System.Diagnostics.CodeAnalysis;

namespace Meditation.Bootstrap.Native.NativeObjectWrappers
{
    internal unsafe class ICLRRuntimeInfoComWrapper : IUnknownComWrapper
    {
        // Virtual method table entries:
        enum MethodTableICLRRuntimeInfo
        {
            // [IUnknown]
            QueryInterface = 0,
            // [IUnknown]
            AddRef = 1,
            // [IUnknown]
            Release = 2,
            // [ICLRRuntimeInfo]
            GetVersionString = 3,
            // [ICLRRuntimeInfo]
            GetRuntimeDirectory = 4,
            // [ICLRRuntimeInfo]
            IsLoaded = 5,
            // [ICLRRuntimeInfo]
            LoadErrorString = 6,
            // [ICLRRuntimeInfo]
            LoadLibrary = 7,
            // [ICLRRuntimeInfo]
            GetProcAddress = 8,
            // [ICLRRuntimeInfo]
            GetInterface = 9,
            // [ICLRRuntimeInfo]
            IsLoadable = 10,
            // [ICLRRuntimeInfo]
            SetDefaultStartupFlags = 11,
            // [ICLRRuntimeInfo]
            GetDefaultStartupFlags = 12,
            // [ICLRRuntimeInfo]
            BindAsLegacyV2Runtime = 13,
            // [ICLRRuntimeInfo]
            IsStarted = 14
        }

        public ICLRRuntimeInfoComWrapper(IntPtr handle)
            : base(handle)
        {

        }

        public int GetInterface<TInterface>(
            Guid rclsid, 
            Guid riid, 
            Func<IntPtr, TInterface> activator, 
            out TInterface? ppUnk)
            where TInterface : IUnknownComWrapper
        {
            var function = GetNthElementInVirtualMethodTable((int)MethodTableICLRRuntimeInfo.GetInterface);
            var result = ((delegate* unmanaged<IntPtr, Guid, Guid, out IntPtr, int>)(function))(
                Handle,
                rclsid,
                riid,
                out var rawInterfaceHandle);

            ppUnk = (result == 0) ? activator(rawInterfaceHandle) : null;
            return result;
        }
    }
}
