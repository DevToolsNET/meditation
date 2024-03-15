using Meditation.PatchingService.Models;

namespace Meditation.PatchingService
{
    public record struct PatchingConfiguration(
        PatchInfo PatchInfo, 
        string NativeBootstrapLibraryPath, 
        string CompanyUniqueIdentifier,
        string NativeBootstrapLibraryLoggingPath,
        string ManagedBootstrapLibraryLoggingPath);
}
