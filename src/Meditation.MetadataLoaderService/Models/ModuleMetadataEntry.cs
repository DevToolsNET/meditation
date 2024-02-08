using dnlib.DotNet;
using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record ModuleMetadataEntry : MetadataEntryBase
    {
        public string Path => ModuleDef.Location;
        internal readonly ModuleDef ModuleDef;

        public ModuleMetadataEntry(ModuleDef moduleDef, ImmutableArray<MetadataEntryBase> children)
            : base(moduleDef.Name, new MetadataToken(moduleDef.MDToken.ToInt32()), children)
        {
            ModuleDef = moduleDef;
        }
    }
}
