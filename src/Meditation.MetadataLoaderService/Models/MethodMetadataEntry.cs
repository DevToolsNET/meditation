﻿using dnlib.DotNet;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Meditation.MetadataLoaderService.Models
{
    public record MethodMetadataEntry : MetadataEntryBase
    {
        internal readonly MethodDef MethodDef;

        internal MethodMetadataEntry(MethodDef methodDef, ImmutableArray<MetadataEntryBase> children)
            : base(methodDef.Name, methodDef.FullName, new MetadataToken(methodDef.MDToken.ToInt32()), children)
        {
            MethodDef = methodDef;
        }

        public string DeclaringTypeFullName => MethodDef.DeclaringType.ReflectionFullName;
        public bool IsStatic => MethodDef.IsStatic;
        public bool HasParameters => MethodDef.HasParamDefs;
        public int ParametersCount => MethodDef.Parameters.Count;
        public bool HasReturnType => MethodDef.HasReturnType;
        public string ModulePath => MethodDef.Module.Location;
        public string AssemblyName => MethodDef.Module.Assembly.FullName;

        public string ToFullDisplayString() => MethodDef.FullName;

        public IEnumerable<string> EnumerateParameterTypeFullNames()
        {
            return MethodDef.Parameters.Where(p => p.IsNormalMethodParameter).Select(p => p.Type.ReflectionFullName);
        }
    }
}
