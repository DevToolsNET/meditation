using System;
using System.Runtime.InteropServices;

namespace Meditation.Core.Utilities
{
    internal class SafeMemoryHandle : SafeHandle
    {
        public static SafeMemoryHandle CreateNew(int bytesCount)
        {
            var unsafeHandle = Marshal.AllocHGlobal(bytesCount);
            return new SafeMemoryHandle(unsafeHandle);
        }

        public static SafeMemoryHandle CreateFromExisting(IntPtr unsafeHandle)
        {
            return new SafeMemoryHandle(unsafeHandle);
        }


        private SafeMemoryHandle(IntPtr unsafeHandle)
            : base(unsafeHandle, true)
        {

        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
                return false;

            Marshal.FreeHGlobal(handle);
            handle = IntPtr.Zero;
            return true;
        }
    }
}
