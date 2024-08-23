using System;
using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public abstract record MetadataEntryBase(
        string Name,
        string FullName,
        MetadataToken? Token,
        ImmutableArray<MetadataEntryBase> Children)
    {
        public MetadataEntryBase? Find(string fullyQualifiedName)
        {
            if (string.Equals(FullName, fullyQualifiedName, StringComparison.Ordinal))
                return this;

            foreach (var child in Children)
            {
                if (child.Find(fullyQualifiedName) is { } result)
                    return result;
            }

            return null;
        }
    }
}
