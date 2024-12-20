using System.Collections.Immutable;
using Meditation.MetadataLoaderService.Models;

namespace Meditation.PatchingService.Models
{
    public sealed record PatchTypeMetadataEntry(string Name, string FullName, ImmutableArray<MetadataEntryBase> Children)
        : MetadataEntryBase(Name, FullName, null, Children);

    public sealed record PatchTypeErrorMetadataEntry(string Error)
        : MetadataEntryBase("<missing>", "<missing>", Token: null, ImmutableArray<MetadataEntryBase>.Empty);
}
