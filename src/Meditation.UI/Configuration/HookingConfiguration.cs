namespace Meditation.UI.Configuration
{
    public record HookingConfiguration(
        string NativeExportedEntryPointSymbol,
        string ManagedBootstrapEntryPointTypeFullName,
        string ManagedBootstrapEntryPointMethod);
}
