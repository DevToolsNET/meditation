using dnlib.DotNet;
using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record ModuleMetadataEntry : MetadataEntryBase
    {
        public string Path => ModuleDef.Location;
        internal readonly ModuleDef ModuleDef;

        public ModuleMetadataEntry(ModuleDef moduleDef, ImmutableArray<MetadataEntryBase> children)
            : base(moduleDef.Name, moduleDef.FullName, Token: null, children)
        {
            ModuleDef = moduleDef;
        }
    }
}
