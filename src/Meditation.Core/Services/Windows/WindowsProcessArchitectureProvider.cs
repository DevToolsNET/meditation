using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Meditation.Common.Services;

namespace Meditation.Core.Services.Windows
{
    internal class WindowsProcessArchitectureProvider : IProcessArchitectureProvider
    {
        // See: https://www.pinvoke.net/default.aspx/kernel32/IsWow64Process.html
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] nint hProcess, [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        public Task<bool> TryGetProcessArchitectureAsync(Process process, [NotNullWhen(true)] out Architecture? architecture, CancellationToken ct)
        {
            if (!TryGetProcessHandle(process, out var handle))
            {
                // The process is not running locally
                architecture = null;
                return Task.FromResult(false);
            }

            var processHandle = handle.Value;
            if (!IsWow64Process(processHandle, out var isWow64))
            {
                // Windows failed to obtain information about the process
                architecture = null;
                return Task.FromResult(false);
            }

            // Assume we are on x86 or x86_64
            architecture = isWow64 ? Architecture.X86 : Architecture.X64;
            return Task.FromResult(true);
        }

        private static bool TryGetProcessHandle(Process process, [NotNullWhen(true)] out IntPtr? handle)
        {
            try
            {
                handle = process.Handle;
                return true;
            }
            catch (InvalidOperationException)
            {
                // FIXME: add logging
                // Process has not been started, or already exited
                handle = null;
                return false;
            }
            catch (NotSupportedException)
            {
                // FIXME: add logging
                // Process is running on a remote computer (unsupported)
                handle = null;
                return false;
            }
        }
    }
}
