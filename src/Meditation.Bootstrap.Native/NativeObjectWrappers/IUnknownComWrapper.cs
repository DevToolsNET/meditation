using System;

namespace Meditation.Bootstrap.Native.NativeObjectWrappers
{
    internal unsafe class IUnknownComWrapper : NativeObjectWrapperBase
    {
        // Virtual method table entries:
        // 0: [IUnknown] QueryInterface
        // 1: [IUnknown] AddRef
        // 2: [IUnknown] Release

        public IUnknownComWrapper(IntPtr handle)
            : base(handle)
        {

        }

        public override uint Release()
        {
            var function = GetNthElementInVirtualMethodTable(2);
            return ((delegate* unmanaged<IntPtr, uint>)(function))(Handle);
        }
    }
}
