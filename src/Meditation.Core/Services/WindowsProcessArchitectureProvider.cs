using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Meditation.Common.Services;

namespace Meditation.Core.Services
{
    internal class WindowsProcessArchitectureProvider : IProcessArchitectureProvider
    {
        // See: https://www.pinvoke.net/default.aspx/kernel32/IsWow64Process.html
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        public bool TryGetProcessArchitecture(Process process, [NotNullWhen(true)] out Architecture? architecture)
        {
            architecture = null;

            try
            {
                // Assume we are on x86 architecture
                var processHandle = process.Handle;
                if (!IsWow64Process(processHandle, out var isWow64))
                    return false;

                architecture = (isWow64) ? Architecture.X86 : Architecture.X64;
                return true;
            }
            catch
            {
                architecture = null;
                return false;
            }
        }
    }
}
