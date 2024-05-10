using Meditation.MetadataLoaderService.Models;
using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService
{
    public interface IDependencyResolver
    {
        ImmutableArray<string> MeditationAssemblies { get; }
        ImmutableArray<string> GetReferencedAssemblies(ModuleMetadataEntry module);
    }
}
