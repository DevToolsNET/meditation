using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService.Services.Windows
{
    internal class WindowsProcessArchitectureProvider : IProcessArchitectureProvider
    {
        public Task<Architecture?> TryGetProcessArchitectureAsync(Process process, CancellationToken ct)
        {
            if (!TryGetProcessHandle(process, out var handle))
            {
                // The process is not running locally
                return Task.FromResult<Architecture?>(null);
            }

            var processHandle = handle.Value;
            if (!IsWow64Process(processHandle, out var isWow64))
            {
                // Windows failed to obtain information about the process
                return Task.FromResult<Architecture?>(null);
            }

            // Assume we are on x86 or x86_64
            var architecture = isWow64 ? Architecture.X86 : Architecture.X64;
            return Task.FromResult<Architecture?>(architecture);
        }

        private static bool TryGetProcessHandle(Process process, [NotNullWhen(true)] out IntPtr? handle)
        {
            try
            {
                handle = process.Handle;
                return true;
            }
            catch (Win32Exception)
            {
                // FIXME [#16]: add logging
                // Most likely insufficient rights (access denied)
                handle = null;
                return false;
            }
            catch (InvalidOperationException)
            {
                // FIXME [#16]: add logging
                // Process has not been started, or already exited
                handle = null;
                return false;
            }
            catch (NotSupportedException)
            {
                // FIXME [#16]: add logging
                // Process is running on a remote computer (unsupported)
                handle = null;
                return false;
            }
            catch (Exception)
            {
                // FIXME [#16]: add logging
                // Unrecognized exception
                handle = null;
                return false;
            }
        }

        // See: https://www.pinvoke.net/default.aspx/kernel32/IsWow64Process.html
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] nint hProcess, [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);
    }
}
