using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record MethodMetadataEntry(string Name, MethodDefinitionToken MethodDefinitionToken, ImmutableArray<MetadataEntryBase> Children)
        : MetadataEntryBase(Name, new MetadataToken(MethodDefinitionToken.Value), Children);
}
