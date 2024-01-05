namespace Meditation.Interop.Windows
{
    /// <summary>
    /// The type of memory operation for a region of pages
    /// </summary>
    public enum MemoryOperationType : uint
    {
        /// <summary>
        /// Allocates memory charges of the specified size (in pages).
        /// Memory won't be allocated until first access. Contents of the pages will be cleared (contains zeroes).
        /// Original constant: MEM_COMMIT
        /// Source: https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-virtualallocex
        /// </summary>
        Commit = 0x1000,

        /// <summary>
        /// Releases previously allocated (and possibly committed) memory pages.
        /// Original constant: MEM_RELEASE
        /// Source: https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-virtualfreeex
        /// </summary>
        Release = 0x8000
    }
}
