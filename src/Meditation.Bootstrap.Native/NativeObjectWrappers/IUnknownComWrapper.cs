using System;

namespace Meditation.Bootstrap.Native.NativeObjectWrappers
{
    internal unsafe class IUnknownComWrapper : NativeObjectWrapperBase
    {
        // Virtual method table entries:
        enum MethodTableIUnknown
        {
            // [IUnknown]
            QueryInterface = 0,
            // [IUnknown]
            AddRef = 1,
            // [IUnknown]
            Release = 2
        }

        public IUnknownComWrapper(IntPtr handle)
            : base(handle)
        {

        }

        public override uint Release()
        {
            var function = GetNthElementInVirtualMethodTable((int)MethodTableIUnknown.Release);
            return ((delegate* unmanaged<IntPtr, uint>)(function))(Handle);
        }
    }
}
