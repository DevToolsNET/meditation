// Modified solution from https://stackoverflow.com/a/46006415

using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Meditation.AttachProcessService.Utilities;

namespace Meditation.AttachProcessService.Services.Windows
{
    // ReSharper disable InconsistentNaming
    internal class WindowsProcessCommandLineProvider : IProcessCommandLineProvider
    {
        public Task<string?> TryGetCommandLineArgumentsAsync(Process process, CancellationToken ct)
        {
            TryGetCommandLineArgumentsCore(process, out var commandLineArguments);
            return Task.FromResult(commandLineArguments);
        }

        private static bool TryGetCommandLineArgumentsCore(Process process, [NotNullWhen(returnValue: true)] out string? commandLine)
        {
            using var safeProcessHandle = OpenProcess(process);

            // Attempt to read basic information about process and user
            if (safeProcessHandle.IsInvalid ||
                !TryReadProcessBasicInformation(safeProcessHandle, out var processInformation) ||
                !TryReadProcessUserInformation(safeProcessHandle, processInformation, out var userProcessInfo))
            {
                commandLine = null;
                return false;
            }

            // Attempt to read command line arguments
            var commandLineBuffer = userProcessInfo.CommandLine.Buffer;
            var commandLineLength = userProcessInfo.CommandLine.MaximumLength;
            return TryReadProcessCommandLine(safeProcessHandle, commandLineBuffer, commandLineLength, out commandLine);
        }

        private static SafeHandle OpenProcess(Process process)
        {
            const bool inheritHandle = false;
            const Win32Native.OpenProcessDesiredAccessFlags processAccessFlags =
                Win32Native.OpenProcessDesiredAccessFlags.PROCESS_QUERY_INFORMATION |
                Win32Native.OpenProcessDesiredAccessFlags.PROCESS_VM_READ;

            var unsafeHandle = Win32Native.OpenProcess(processAccessFlags, inheritHandle, (uint)process.Id);
            return new SafeProcessHandle(unsafeHandle, true);
        }

        private static bool TryReadProcessBasicInformation(SafeHandle safeProcessHandle, out Win32Native.ProcessBasicInformation processInformation)
        {
            const uint processInformationClass = Win32Native.PROCESS_BASIC_INFORMATION;
            var processInformationLength = Marshal.SizeOf<Win32Native.ProcessBasicInformation>();
            SafeHandle? processInformationSafeHandle = null;
            processInformation = default;

            try
            {
                processInformationSafeHandle = SafeMemoryHandle.CreateNew(processInformationLength);
                var result = Win32Native.NtQueryInformationProcess(
                    safeProcessHandle.DangerousGetHandle(),
                    processInformationClass,
                    processInformationSafeHandle.DangerousGetHandle(),
                    (uint)processInformationLength,
                    out _);

                if (result != 0)
                    return false;

                processInformation = Marshal.PtrToStructure<Win32Native.ProcessBasicInformation>(processInformationSafeHandle.DangerousGetHandle());
                return processInformation.PebBaseAddress != IntPtr.Zero;
            }
            finally
            {
                processInformationSafeHandle?.Dispose();
            }
        }

        private static bool TryReadProcessUserInformation(SafeHandle safeProcessHandle, Win32Native.ProcessBasicInformation processInfo, out Win32Native.RtlUserProcessParameters userProcessParameters)
        {
            userProcessParameters = default;
            return TryReadStructFromProcessMemory<Win32Native.PEB>(safeProcessHandle.DangerousGetHandle(), processInfo.PebBaseAddress, out var pebInfo) && 
                   TryReadStructFromProcessMemory(safeProcessHandle.DangerousGetHandle(), pebInfo.ProcessParameters, out userProcessParameters);
        }

        private static bool TryReadProcessCommandLine(SafeHandle safeProcessHandle, IntPtr commandLineUnsafeHandle, int commandLineLength, out string? commandLine)
        {
            commandLine = null;
            SafeHandle? safeCommandLineBufferHandle = null;

            try
            {
                safeCommandLineBufferHandle = SafeMemoryHandle.CreateNew(commandLineLength);
                if (safeCommandLineBufferHandle.IsInvalid)
                    return false;

                if (!Win32Native.ReadProcessMemory(
                        safeProcessHandle.DangerousGetHandle(),
                        commandLineUnsafeHandle,
                        safeCommandLineBufferHandle.DangerousGetHandle(),
                        (uint)commandLineLength,
                        out _))
                {
                    // Unable to read command line information
                    return false;
                }

                commandLine = Marshal.PtrToStringUni(safeCommandLineBufferHandle.DangerousGetHandle());
                return true;
            }
            finally
            {
                safeCommandLineBufferHandle?.Dispose();
            }
        }

        private static bool TryReadStructFromProcessMemory<TStruct>(nint hProcess, nint lpBaseAddress, out TStruct? val)
        {
            val = default;
            var structSize = Marshal.SizeOf<TStruct>();
            var mem = Marshal.AllocHGlobal(structSize);
            try
            {
                if (Win32Native.ReadProcessMemory(hProcess, lpBaseAddress, mem, (uint)structSize, out var len) && len == structSize)
                {
                    val = Marshal.PtrToStructure<TStruct>(mem);
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(mem);
            }
            return false;
        }

        private static class Win32Native
        {
            public const uint PROCESS_BASIC_INFORMATION = 0;

            [Flags]
            public enum OpenProcessDesiredAccessFlags : uint
            {
                PROCESS_VM_READ = 0x0010,
                PROCESS_QUERY_INFORMATION = 0x0400,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct ProcessBasicInformation
            {
                public nint Reserved1;
                public nint PebBaseAddress;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public nint[] Reserved2;
                public nint UniqueProcessId;
                public nint Reserved3;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct UnicodeString
            {
                public ushort Length;
                public ushort MaximumLength;
                public nint Buffer;
            }

            // This is not the real struct!
            // I faked it to get ProcessParameters address.
            // Actual struct definition:
            // https://learn.microsoft.com/en-us/windows/win32/api/winternl/ns-winternl-peb
            [StructLayout(LayoutKind.Sequential)]
            public struct PEB
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public nint[] Reserved;
                public nint ProcessParameters;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RtlUserProcessParameters
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] Reserved1;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public nint[] Reserved2;
                public UnicodeString ImagePathName;
                public UnicodeString CommandLine;
            }

            [DllImport("ntdll.dll")]
            public static extern uint NtQueryInformationProcess(
                nint ProcessHandle,
                uint ProcessInformationClass,
                nint ProcessInformation,
                uint ProcessInformationLength,
                out uint ReturnLength);

            [DllImport("kernel32.dll")]
            public static extern nint OpenProcess(
                OpenProcessDesiredAccessFlags dwDesiredAccess,
                [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
                uint dwProcessId);

            [DllImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ReadProcessMemory(
                nint hProcess, nint lpBaseAddress, nint lpBuffer,
                uint nSize, out uint lpNumberOfBytesRead);
        }
    }
}
