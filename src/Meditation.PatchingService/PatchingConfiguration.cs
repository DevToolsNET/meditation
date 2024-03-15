using System.Reflection;
using Meditation.PatchingService.Models;

namespace Meditation.PatchingService
{
    public record struct PatchingConfiguration(
        PatchInfo PatchInfo,
        string NativeBootstrapLibraryPath,
        string NativeExportedEntryPointSymbol,
        string ManagedBootstrapEntryPointTypeFullName,
        string ManagedBootstrapEntryPointMethod,
        string CompanyUniqueIdentifier,
        string NativeBootstrapLibraryLoggingPath,
        string ManagedBootstrapLibraryLoggingPath)
    {
        public static string ConstructHookArgs(Assembly patchAssembly, PatchingConfiguration configuration)
        {
            return string.Join('#',
                configuration.NativeBootstrapLibraryLoggingPath,
                configuration.ManagedBootstrapLibraryLoggingPath,
                configuration.CompanyUniqueIdentifier,
                patchAssembly.Location,
                configuration.ManagedBootstrapEntryPointTypeFullName,
                configuration.ManagedBootstrapEntryPointMethod,
                configuration.PatchInfo.Path);
        }
    }
}
