namespace Meditation.Interop
{
    public enum ErrorCode : uint
    {
        Ok = 0,
        InternalError = 0xDEAD_0001,
        InvalidArguments_HookArgs_PointerIsNull = 0xDEAD_0002,
        InvalidArguments_HookArgs_PointerIsNotAValidNativeWideString = 0xDEAD_0003,
        InvalidArguments_HookArgs_CouldNotParse = 0xDEAD_0004,
        NotFound_Clr = 0xDEAD_0005,
        NotFound_MetaHost = 0xDEAD_0006,
        NotFound_Host = 0xDEAD_0007,
        NotSupported_OperatingSystem = 0xDEAD_0008,
        NotSupported_Process = 0xDEAD_0009,
        NotImplemented_OperatingSystem = 0xDEAD_0010,
        RuntimeError_Marshalling_HookArguments = 0xDEAD_0011,
        RuntimeError_Invocation_ExecuteInDefaultAppDomain = 0xDEAD_0012,
        RuntimeError_Invocation_ManagedHook = 0xDEAD_0013
    }
}
