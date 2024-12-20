using System.Collections.Immutable;
using Meditation.MetadataLoaderService.Models;

namespace Meditation.PatchingService.Models
{
    public sealed record PatchAssemblyMetadataEntry(string Name, string FullName, ImmutableArray<MetadataEntryBase> Children)
        : MetadataEntryBase(Name, FullName, Token: null, Children);

    public sealed record PatchAssemblyErrorMetadataEntry(string Error)
        : MetadataEntryBase("<missing>", "<missing>", Token: null, ImmutableArray<MetadataEntryBase>.Empty);
}
