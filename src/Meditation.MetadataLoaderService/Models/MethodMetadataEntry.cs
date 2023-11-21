using dnlib.DotNet;
using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record MethodMetadataEntry : MetadataEntryBase
    {
        private readonly MethodDef _methodDef;

        internal MethodMetadataEntry(string name, MethodDefinitionToken methodDefinitionToken, ImmutableArray<MetadataEntryBase> children, MethodDef methodDef)
            : base(name, new MetadataToken(methodDefinitionToken.Value), children)
        {
            _methodDef = methodDef;
        }

        public string? ReturnType => _methodDef.ReturnType?.FullName;
        public string DeclaringTypeFullName => _methodDef.DeclaringType.FullName;
        public bool IsStatic => _methodDef.IsStatic;
        public bool HasParameters => _methodDef.HasParamDefs;
        public bool HasReturnType => _methodDef.HasReturnType;

        public string ToFullDisplayString() => _methodDef.FullName;
    }
}
