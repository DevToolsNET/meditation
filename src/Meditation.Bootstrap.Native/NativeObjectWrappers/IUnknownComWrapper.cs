using System;

namespace Meditation.Bootstrap.Native.NativeObjectWrappers
{
    internal abstract unsafe class IUnknownComWrapper : IDisposable
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

        protected IntPtr Handle { get; private set; }

        protected IUnknownComWrapper(IntPtr handle)
        {
            Handle = handle;
        }

        ~IUnknownComWrapper()
        {
            Dispose(false);
        }

        protected void* GetNthElementInVirtualMethodTable(int index)
        {
            var instance = Handle;
            var vtable = *(void***)instance;
            return *(vtable + index);
        }

        private uint Release()
        {
            var function = GetNthElementInVirtualMethodTable((int)MethodTableIUnknown.Release);
            return ((delegate* unmanaged<IntPtr, uint>)(function))(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Handle == IntPtr.Zero)
                return;

            Release();
            Handle = IntPtr.Zero;
        }
    }
}
