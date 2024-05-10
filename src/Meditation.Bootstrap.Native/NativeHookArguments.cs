using Meditation.Interop;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System;

namespace Meditation.Bootstrap.Native
{
    public record NativeHookArguments(
        string LoggingFileNameNativeLibrary,
        string LoggingFileNameManagedLibrary,
        string UniqueIdentifier,
        string AssemblyPath,
        string TypeFullName,
        string MethodName,
        string Argument)
    {
        public static bool TryParse(IntPtr nativeWideStringHookArgs, out NativeHookErrorCode error, [NotNullWhen(true)] out NativeHookArguments? hookArgs)
        {
            hookArgs = null;

            // Ensure the pointer is valid
            if (nativeWideStringHookArgs == IntPtr.Zero)
            {
                error = NativeHookErrorCode.InvalidArguments_HookArgs_PointerIsNull;
                return false;
            }

            // Ensure the pointer was actually pointing to a string
            var rawArgs = Marshal.PtrToStringUni(nativeWideStringHookArgs);
            if (rawArgs == null)
            {
                error = NativeHookErrorCode.InvalidArguments_HookArgs_PointerIsNotAValidNativeWideString;
                return false;
            }

            // Ensure there are enough elements
            var tokens = rawArgs.Split("#");
            if (tokens.Length < 7)
            {
                error = NativeHookErrorCode.InvalidArguments_HookArgs_CouldNotParse;
                return false;
            }

            hookArgs = new NativeHookArguments(
                LoggingFileNameNativeLibrary: tokens[0],
                LoggingFileNameManagedLibrary: tokens[1],
                UniqueIdentifier: tokens[2],
                AssemblyPath: tokens[3],
                TypeFullName: tokens[4],
                MethodName: tokens[5],
                Argument: tokens[6]);

            error = NativeHookErrorCode.Ok;
            return true;
        }
    }
}
