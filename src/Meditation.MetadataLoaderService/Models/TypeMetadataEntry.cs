using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record TypeMetadataEntry(string Name, string FullName, TypeDefinitionToken TypeDefinitionToken, ImmutableArray<MetadataEntryBase> Children)
        : MetadataEntryBase(Name, new MetadataToken(TypeDefinitionToken.Value), Children);
}
