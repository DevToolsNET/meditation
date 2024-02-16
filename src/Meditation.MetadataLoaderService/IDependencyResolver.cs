using System.Collections.Immutable;
using System.IO;
using Meditation.MetadataLoaderService.Models;

namespace Meditation.MetadataLoaderService
{
    public interface IDependencyResolver
    {
        ImmutableArray<string> MeditationAssemblies { get; }

        ModuleMetadataEntry GetCoreLibrary(MethodMetadataEntry method);
        ImmutableArray<string> GetReferencedAssemblies(ModuleMetadataEntry module);
        DirectoryInfo GetReferenceAssembliesFolder(ModuleMetadataEntry coreLibraryModule);
    }
}
