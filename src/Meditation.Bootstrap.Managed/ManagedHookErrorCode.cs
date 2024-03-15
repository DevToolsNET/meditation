namespace Meditation.Bootstrap.Managed
{
    public enum ManagedHookErrorCode : uint
    {
        Ok = 0,
        InternalError = 0xDEAD_0001,
        InvalidArguments_HookArgs_CouldNotParse = 0xDEAD_0002,
        PatchAssemblyLoadException = 0xDEAD_0003,
        UnhandledException_ApplyingPatches = 0xDEAD_0004,
    }
}
