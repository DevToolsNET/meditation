using Meditation.Interop;
using System.Runtime.InteropServices;

namespace Meditation.Bootstrap.Native.Utils
{
    internal static class MarshalingUtils
    {
        public static SafeHandle ConvertStringToNativeLpcwstr(string input)
        {
            return new GenericSafeHandle(
                acquireDelegate: () => Marshal.StringToHGlobalUni(input),
                releaseDelegate: ptr =>
                {
                    Marshal.FreeHGlobal(ptr);
                    return true;
                });
        }
    }
}
