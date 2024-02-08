using System;

namespace Meditation.Interop
{
    public class GenericSafeHandle : System.Runtime.InteropServices.SafeHandle
    {
        private readonly Func<nint, bool> _releaseDelegate;

        public GenericSafeHandle(Func<nint> acquireDelegate, Func<nint, bool> releaseDelegate, bool ownsHandle = true)
            : base(nint.Zero, ownsHandle: ownsHandle)
        {
            _releaseDelegate = releaseDelegate;
            SetHandle(acquireDelegate());
        }

        public override bool IsInvalid => handle == nint.Zero;

        protected override bool ReleaseHandle() => _releaseDelegate(handle);
    }
}
