using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Meditation.Interop.Linux;

public static partial class DynamicLinking
{
    public const int DLOPEN_RTLD_LAZY = 0x1;
    public const int DLOPEN_RTLD_NOW = 0x2;
    public const int DLOPEN_RTLD_GLOBAL = 0x100;
    
    public static SafeHandle OpenDynamicSharedObject(string filename, int flags)
    {
        IntPtr AcquireFunction() => NativeBindings.dlopen(filename, flags);
        return new GenericSafeHandle(AcquireFunction, static _ => true, ownsHandle: false);
    }
    
    public static SafeHandle GetSymbolAddress(SafeHandle handle, string symbol)
    {
        IntPtr AcquireFunction() => NativeBindings.dlsym(handle.DangerousGetHandle(), symbol);
        return new GenericSafeHandle(AcquireFunction, static _ => true, ownsHandle: false);
    }
    
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Matches native library function names")]
    private static partial class NativeBindings
    {
        [LibraryImport("libdl.so.2", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr dlopen(string filename, int flags);
        
        [LibraryImport("libdl.so.2", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr dlsym(IntPtr handle, string symbol);
    }
}