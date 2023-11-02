using System;

namespace Meditation.Bootstrap.Native.NativeObjectWrappers
{
    internal abstract unsafe class NativeObjectWrapperBase : IDisposable
    {
        protected IntPtr Handle { get; private set; }

        protected NativeObjectWrapperBase(IntPtr handle)
        {
            Handle = handle;
        }

        public abstract uint Release();

        protected void* GetNthElementInVirtualMethodTable(int index)
        {
            var instance = Handle;
            var vtable = *(void***)instance;
            return *(vtable + index);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Handle != IntPtr.Zero)
            {
                Release();
                Handle = IntPtr.Zero;
            }
        }

        ~NativeObjectWrapperBase()
        {
            Dispose(false);
        }
    }
}
