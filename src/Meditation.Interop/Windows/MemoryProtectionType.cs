namespace Meditation.Interop.Windows
{
    /// <summary>
    /// The type of memory protection for a region of pages.
    /// Determines set of operations (R/W/E) that can be performed on the region of pages.
    /// </summary>
    public enum MemoryProtectionType : uint
    {
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08,
    }
}
