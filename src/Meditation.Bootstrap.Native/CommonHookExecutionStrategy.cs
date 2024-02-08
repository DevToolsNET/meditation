using Meditation.Bootstrap.Native.NativeObjectWrappers;
using Meditation.Bootstrap.Native.Utils;
using Meditation.Interop;

namespace Meditation.Bootstrap.Native
{
    internal static class CommonHookExecutionStrategy
    {
        public static ErrorCode ExecuteHook(ICLRRuntimeHostComWrapper runtimeHost, HookArguments args)
        {
            using var assemblyPathNativeStringHandle = MarshalingUtils.ConvertStringToNativeLpcwstr(args.AssemblyPath);
            using var typeFullNameNativeStringHandle = MarshalingUtils.ConvertStringToNativeLpcwstr(args.TypeFullName);
            using var methodNameNativeStringHandle = MarshalingUtils.ConvertStringToNativeLpcwstr(args.MethodName);
            using var argumentNativeStringHandle = MarshalingUtils.ConvertStringToNativeLpcwstr(args.Argument);
            if (argumentNativeStringHandle.IsInvalid ||
                typeFullNameNativeStringHandle.IsInvalid ||
                methodNameNativeStringHandle.IsInvalid ||
                argumentNativeStringHandle.IsInvalid)
            {
                // Could not marshall arguments
                // FIXME [#16]: logging
                return ErrorCode.RuntimeError_Marshalling_HookArguments;
            }

            var result = runtimeHost.ExecuteInDefaultAppDomain(
                assemblyPathNativeStringHandle,
                typeFullNameNativeStringHandle,
                methodNameNativeStringHandle,
                argumentNativeStringHandle,
                out var exitCode);
            if (result != 0)
            {
                // Error during while trying to execute managed hook's entrypoint
                // FIXME [#16]: log return value
                return ErrorCode.RuntimeError_Invocation_ExecuteInDefaultAppDomain;
            }

            if (exitCode != 0)
            {
                // Error during execution of managed hook's entrypoint
                // FIXME [#16]: log exit code
                return ErrorCode.RuntimeError_Invocation_ManagedHook;
            }

            // Success
            return ErrorCode.Ok;
        }
    }
}
