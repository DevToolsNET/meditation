using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security;
using System.Text;

namespace Meditation.InjectorService.Services.Windows
{
    internal partial class WindowsProcessInjector : IProcessInjector
    {
        public bool TryInjectModuleToProcess(int pid, string assemblyPath, out nint remoteModuleHandle)
        {
            // Obtain process handle
            var processHandle = Win32Native.OpenProcess((uint)Win32Native.ProcessAccessFlags.All, bInheritHandle: false, (uint)pid);
            if (processHandle == IntPtr.Zero)
            {
                // Could not open process
                // FIXME: logging
                remoteModuleHandle = IntPtr.Zero;
                return false;
            }

            // Attempt to allocate memory in target process to store assembly path
            // hProcess: handle to target process
            // lpAddress: optional parameter to specify starting address (we do not care)
            // dwSize: size to allocate in bytes expressed in UInt32
            // flAllocationType: allocation flags - COMMIT as we immediately need the memory
            // flProtect: memory protection flags - ReadWrite (we write, target process reads)
            var nativeAssemblyPathStringBufferSize = assemblyPath.Length * sizeof(char) + 1;
            var remoteAssemblyBufferPtr = Win32Native.VirtualAllocEx(
                hProcess: processHandle,
                lpAddress: IntPtr.Zero,
                dwSize: (uint)nativeAssemblyPathStringBufferSize,
                flAllocationType: (uint)Win32Native.AllocationType.Commit,
                flProtect: (uint)Win32Native.MemoryProtection.ReadWrite);
            if (remoteAssemblyBufferPtr == IntPtr.Zero)
            {
                // Unable to allocate memory in target process
                // FIXME: logging
                remoteModuleHandle = IntPtr.Zero;
                return false;
            }

            // Write assembly path to target process
            // Convert assembly path to ASCII string
            var nativeAssemblyStringData = Encoding.Unicode.GetBytes(assemblyPath + '\0');
            if (!Win32Native.WriteProcessMemory(
                    hProcess: processHandle,
                    lpBaseAddress: remoteAssemblyBufferPtr,
                    lpBuffer: nativeAssemblyStringData,
                    nSize: nativeAssemblyPathStringBufferSize,
                    lpNumberOfBytesWritten: out _))
            {
                // Unable to write memory of target process
                // FIXME: logging
                remoteModuleHandle = IntPtr.Zero;
                return false;
            }

            if ((Win32Native.GetModuleHandle("kernel32") is { } kernel32ModuleHandle && kernel32ModuleHandle == IntPtr.Zero) ||
                (Win32Native.GetProcAddress(kernel32ModuleHandle, "LoadLibraryW") is { } loadLibraryFunctionPtr && loadLibraryFunctionPtr == IntPtr.Zero))
            {
                // Unable to obtain handle to kernel32 or one of its defined symbols
                // FIXME: logging
                remoteModuleHandle = IntPtr.Zero;
                return false;
            }

            // Create thread in target process and use it to load the module
            // hProcess: handle to target process
            // lpThreadAttributes: pointer to security attributes (nullptr means default)
            // dwStackSize: initial stack size in bytes (0 means default for the executable)
            // lpStartAddress: pointer to function within remote process to execute
            // lpParameter: pointer to argument for start function
            // dwCreationFlags: flags controlling thread creation (0 means start immediately)
            // lpThreadId: thread identifier
            var remoteThreadHandle = Win32Native.CreateRemoteThread(
                hProcess: processHandle,
                lpThreadAttributes: IntPtr.Zero,
                dwStackSize: 0,
                lpStartAddress: loadLibraryFunctionPtr,
                lpParameter: remoteAssemblyBufferPtr,
                dwCreationFlags: 0,
                lpThreadId: out _);
            if (remoteThreadHandle == IntPtr.Zero)
            {
                // Could not create thread in target process
                // FIXME: logging
                remoteModuleHandle = IntPtr.Zero;
                return false;
            }

            // Wait for the remote thread to finish execution
            // hHandle: handle to remote thread
            // dwMilliseconds: maximum wait period in milliseconds, or 0xFFFFFFFF (infinite)
            const uint infiniteWait = 0xFFFFFFFF;
            if (Win32Native.WaitForSingleObject(
                    hHandle: remoteThreadHandle,
                    dwMilliseconds: infiniteWait) != 0)
            {
                // Error while waiting for remote thread to finish
                // FIXME: logging
                remoteModuleHandle = IntPtr.Zero;
                return false;
            }

            // Obtain handle to injected module though thread exit code
            // hThread: handle to remote thread
            // lpExitCode: [out] pointer to thread exit code
            if (!Win32Native.GetExitCodeThread(
                    hThread: remoteThreadHandle,
                    lpExitCode: out remoteModuleHandle))
            {
                // Error while obtaining thread's exit code
                // FIXME: logging
                remoteModuleHandle = IntPtr.Zero;
                return false;
            }

            // Cleanup
            Win32Native.VirtualFreeEx(processHandle, remoteAssemblyBufferPtr, nativeAssemblyPathStringBufferSize, Win32Native.AllocationType.Release);
            Win32Native.CloseHandle(remoteThreadHandle);
            Win32Native.CloseHandle(processHandle);
            return true;
        }

        public bool TryExecuteStaticMethodInDefaultProcessAppDomain(int pid, string assemblyPath, string fullTypeName, string methodName, string arg, out int returnCode)
        {
            throw new NotImplementedException();
        }

        private static partial class Win32Native
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
            public static partial bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, AllocationType dwFreeType);

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
            public static partial bool GetExitCodeThread(IntPtr hThread, out IntPtr lpExitCode);

            // Source: http://pinvoke.net/default.aspx/kernel32/OpenProcess.html
            [LibraryImport("kernel32.dll", SetLastError = true)]
            public static partial IntPtr OpenProcess(uint processAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint processId);

            // Source: http://pinvoke.net/default.aspx/kernel32/CloseHandle.html
            [LibraryImport("kernel32.dll", SetLastError = true)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool CloseHandle(IntPtr hObject);

            [Flags]
            public enum AllocationType : uint
            {
                Commit = 0x1000,
                Reserve = 0x2000,
                Decommit = 0x4000,
                Release = 0x8000,
                Reset = 0x80000,
                Physical = 0x400000,
                TopDown = 0x100000,
                WriteWatch = 0x200000,
                LargePages = 0x20000000
            }

            [Flags]
            public enum MemoryProtection : uint
            {
                Execute = 0x10,
                ExecuteRead = 0x20,
                ExecuteReadWrite = 0x40,
                ExecuteWriteCopy = 0x80,
                NoAccess = 0x01,
                ReadOnly = 0x02,
                ReadWrite = 0x04,
                WriteCopy = 0x08,
                GuardModifierflag = 0x100,
                NoCacheModifierflag = 0x200,
                WriteCombineModifierflag = 0x400
            }

            [Flags]
            public enum ProcessAccessFlags : uint
            {
                All = 0x001F0FFF,
                Terminate = 0x00000001,
                CreateThread = 0x00000002,
                VirtualMemoryOperation = 0x00000008,
                VirtualMemoryRead = 0x00000010,
                VirtualMemoryWrite = 0x00000020,
                DuplicateHandle = 0x00000040,
                CreateProcess = 0x000000080,
                SetQuota = 0x00000100,
                SetInformation = 0x00000200,
                QueryInformation = 0x00000400,
                QueryLimitedInformation = 0x00001000,
                Synchronize = 0x00100000
            }
        }
    }
}
