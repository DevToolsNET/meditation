using Meditation.Interop;
using Meditation.Interop.Windows;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Meditation.Bootstrap.Native
{
    public static class EntryPoint
    {
        // Note: this method is dynamically invoked by tests
        [UnmanagedCallersOnly(EntryPoint = "MeditationSanityCheck")]
        public static uint SanityCheck(IntPtr _)
        {
            return 0xABCD_EF98;
        }

        // Note: this method is dynamically invoked by Meditation.InjectorService
        [UnmanagedCallersOnly(EntryPoint = "MeditationInitialize")]
        public static uint NativeEntryPoint(IntPtr nativeWideStringHookArgs)
        {
            if (!TryParseHooksArgs(nativeWideStringHookArgs, out var hookArguments))
                return (uint)ErrorCode.InvalidArguments;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return (uint)NativeEntryPointWindows(hookArguments);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return (uint)ErrorCode.NotImplemented;
            
            // Unknown platform
            return (uint)ErrorCode.NotSupported;
        }

        private static bool TryParseHooksArgs(IntPtr nativeWideStringHookArgs, [NotNullWhen(returnValue: true)] out HookArguments? hookArgs)
        {
            hookArgs = null;
            // Ensure the pointer is valid
            if (nativeWideStringHookArgs == IntPtr.Zero)
                return false;

            var rawArgs = Marshal.PtrToStringUni(nativeWideStringHookArgs);

            // Ensure the pointer was actually pointing to a string
            if (rawArgs == null)
                return false;

            // Ensure there are enough elements
            var tokens = rawArgs.Split("#");
            if (tokens.Length < 4)
                return false;

            hookArgs = new HookArguments(
                AssemblyPath: tokens[0],
                TypeFullName: tokens[1], 
                MethodName: tokens[2], 
                Argument: tokens[3]);
            return true;
        }

        private static ErrorCode NativeEntryPointWindows(HookArguments arguments)
        {
            const string coreClrModule = "coreclr.dll";
            const string mscoreeModule = "mscoree.dll";

            // Test for .NET Core application
            using var coreClrModuleHandle = Kernel32.GetModuleHandle(coreClrModule);
            if (!coreClrModuleHandle.IsInvalid)
                return NetCoreHookingStrategy.TryInitializeWindowsNetCoreProcess(coreClrModuleHandle, arguments);

            // Test for .NET Framework application
            using var mscoreeModuleHandle = Kernel32.GetModuleHandle(mscoreeModule);
            if (!mscoreeModuleHandle.IsInvalid)
            {
                // TODO: implement .NET Framework support
                return ErrorCode.NotImplemented;
            }

            // Attempt to inject an unsupported process
            return ErrorCode.ClrNotFound;
        }
    }
}