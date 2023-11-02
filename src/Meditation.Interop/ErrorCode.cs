namespace Meditation.Interop
{
    public enum ErrorCode : uint
    {
        Ok = 0,
        InvalidArguments = 0xDEAD_0001,
        RuntimeError = 0xDEAD_0002,
        NotSupported = 0xDEAD_0003,
        NotImplemented = 0xDEAD_0004,
        ClrNotFound = 0xDEAD_0005,
        MetaHostNotFound = 0xDEAD_0006,
        HostNotFound = 0xDEAD_0007
    }
}
