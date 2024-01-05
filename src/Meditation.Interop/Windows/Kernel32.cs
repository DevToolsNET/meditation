using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;
using System.Security;
using System;

namespace Meditation.Interop.Windows
{
    public static partial class Kernel32
    {
        public static SafeHandle LoadLibraryW(string library)
        {
            IntPtr AcquireFunction() => NativeBindings.LoadLibraryW(library);
            return new GenericSafeHandle(AcquireFunction, static _ => true, ownsHandle: false);
        }

        public static SafeHandle OpenProcess(ProcessAccessFlags processAccess, uint processId)
        {
            IntPtr AcquireFunction() => NativeBindings.OpenProcess((uint)processAccess, bInheritHandle: false, processId);
            return new GenericSafeHandle(AcquireFunction, NativeBindings.CloseHandle);
        }

        public static SafeHandle VirtualAllocEx(SafeHandle processHandle, uint bytesCount, MemoryProtectionType protectionType)
        {
            IntPtr AcquireFunction() => NativeBindings.VirtualAllocEx(
                hProcess: processHandle.DangerousGetHandle(), 
                lpAddress: IntPtr.Zero, 
                dwSize: bytesCount, 
                flAllocationType: (uint)MemoryOperationType.Commit, 
                flProtect: (uint)protectionType);

            bool ReleaseFunction(IntPtr handle) => NativeBindings.VirtualFreeEx(
                hProcess: processHandle.DangerousGetHandle(), 
                lpAddress: handle, 
                dwSize: bytesCount, 
                dwFreeType: MemoryOperationType.Release);

            return new GenericSafeHandle(AcquireFunction, ReleaseFunction);
        }

        public static bool WriteProcessMemory(SafeHandle processHandle, SafeHandle memoryHandle, byte[] data, out uint writtenBytes)
        {
            var result = NativeBindings.WriteProcessMemory(
                hProcess: processHandle.DangerousGetHandle(),
                lpBaseAddress: memoryHandle.DangerousGetHandle(),
                lpBuffer: data,
                nSize: data.Length,
                lpNumberOfBytesWritten: out var written);
            writtenBytes = (uint)written;
            return result;
        }

        public static SafeHandle GetModuleHandle(string moduleName)
        {
            IntPtr AcquireFunction() => NativeBindings.GetModuleHandle(moduleName);
            return new GenericSafeHandle(AcquireFunction, static _ => true, ownsHandle: false);
        }

        public static SafeHandle GetProcAddress(SafeHandle moduleHandle, string procedureName)
        {
            IntPtr AcquireFunction() => NativeBindings.GetProcAddress(moduleHandle.DangerousGetHandle(), procedureName);
            return new GenericSafeHandle(AcquireFunction, static _ => true, ownsHandle: false);
        }

        public static SafeHandle CreateRemoteThread(SafeHandle processHandle, SafeHandle threadStart, SafeHandle? args)
        {
            // hProcess: handle to target process
            // lpThreadAttributes: pointer to security attributes (nullptr means default)
            // dwStackSize: initial stack size in bytes (0 means default for the executable)
            // lpStartAddress: pointer to function within remote process to execute
            // lpParameter: pointer to argument for start function
            // dwCreationFlags: flags controlling thread creation (0 means start immediately)
            // lpThreadId: thread identifier
            IntPtr AcquireFunction() => NativeBindings.CreateRemoteThread(
                hProcess: processHandle.DangerousGetHandle(),
                lpThreadAttributes: IntPtr.Zero,
                dwStackSize: 0,
                lpStartAddress: threadStart.DangerousGetHandle(),
                lpParameter: args?.DangerousGetHandle() ?? IntPtr.Zero,
                dwCreationFlags: 0,
                lpThreadId: out _);

            return new GenericSafeHandle(AcquireFunction, NativeBindings.CloseHandle);
        }

        public static bool WaitForSingleObject(SafeHandle handle, TimeSpan? timeout = null)
        {
            const uint infiniteWait = 0xFFFFFFFF;
            var timeoutMillis = timeout.HasValue ? (uint)timeout.Value.Milliseconds : infiniteWait;
            return NativeBindings.WaitForSingleObject(handle.DangerousGetHandle(), timeoutMillis) == 0;
        }

        public static bool GetExitCodeThread(SafeHandle threadHandle, out uint returnValue)
        {
            return NativeBindings.GetExitCodeThread(threadHandle.DangerousGetHandle(), out returnValue);
        }

        private static partial class NativeBindings
        {
            // Source: http://pinvoke.net/default.aspx/kernel32.WriteProcessMemory
            [LibraryImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

            // Source: http://pinvoke.net/default.aspx/kernel32.VirtualAllocEx
            [LibraryImport("kernel32.dll", SetLastError = true)]
            public static partial IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

            [LibraryImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, MemoryOperationType dwFreeType);

            // Source: http://pinvoke.net/default.aspx/kernel32/GetModuleHandle.html
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

            // Source: https://www.pinvoke.net/default.aspx/kernel32/GetProcAddress.html
            [LibraryImport("kernel32", SetLastError = true, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
            public static partial IntPtr GetProcAddress(IntPtr hModule, string procName);

            // Source: https://www.pinvoke.net/default.aspx/kernel32.createremotethread
            [LibraryImport("kernel32.dll")]
            public static partial IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress,
                IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

            // Source: http://pinvoke.net/default.aspx/kernel32.waitforsingleobject
            [LibraryImport("kernel32.dll", SetLastError = true)]
            public static partial uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

            // Source: http://pinvoke.net/default.aspx/kernel32.getexitcodethread
            [LibraryImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

            // Source: http://pinvoke.net/default.aspx/kernel32/OpenProcess.html
            [LibraryImport("kernel32.dll", SetLastError = true)]
            public static partial IntPtr OpenProcess(uint processAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint processId);

            // Source: http://pinvoke.net/default.aspx/kernel32/CloseHandle.html
            [LibraryImport("kernel32.dll", SetLastError = true)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool CloseHandle(IntPtr hObject);

            [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
            public static partial IntPtr LoadLibraryW(string librayName);
        }
    }
}
