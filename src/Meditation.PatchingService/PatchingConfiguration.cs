using System.Reflection;

namespace Meditation.PatchingService
{
    public record struct PatchingConfiguration(
        string PatchAssemblyPath,
        string NativeBootstrapLibraryPath,
        string NativeExportedEntryPointSymbol,
        string ManagedBootstrapEntryPointTypeFullName,
        string ManagedBootstrapEntryPointMethod,
        string CompanyUniqueIdentifier,
        string NativeBootstrapLibraryLoggingPath,
        string ManagedBootstrapLibraryLoggingPath)
    {
        public static string ConstructArgs(Assembly patchAssembly, PatchingConfiguration configuration)
        {
            return string.Join('#',
                configuration.NativeBootstrapLibraryLoggingPath,
                configuration.ManagedBootstrapLibraryLoggingPath,
                configuration.CompanyUniqueIdentifier,
                patchAssembly.Location,
                configuration.ManagedBootstrapEntryPointTypeFullName,
                configuration.ManagedBootstrapEntryPointMethod,
                configuration.PatchAssemblyPath);
        }
    }
}
