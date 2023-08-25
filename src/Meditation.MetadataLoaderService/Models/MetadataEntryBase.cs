using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public abstract record MetadataEntryBase(
        string Name,
        ImmutableArray<MetadataEntryBase> Children);
}
