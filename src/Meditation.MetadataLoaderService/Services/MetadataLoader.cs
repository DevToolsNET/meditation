using dnlib.DotNet;
using Meditation.MetadataLoaderService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Services
{
    internal class MetadataLoader : IMetadataLoader
    {
        private readonly ConcurrentDictionary<string, AssemblyDef> _loadedAssemblies;
        private readonly ConcurrentDictionary<string, AssemblyMetadataEntry> _metadataModels;
        private readonly object _syncObject;

        public MetadataLoader()
        {
            _loadedAssemblies = new ConcurrentDictionary<string, AssemblyDef>();
            _metadataModels = new ConcurrentDictionary<string, AssemblyMetadataEntry>();
            _syncObject = new object();
        }

        public AssemblyMetadataEntry LoadMetadataFromAssembly(string path)
        {
            // Fast-path: metadata model is already constructed
            if (_metadataModels.TryGetValue(path, out var metadataModel))
                return metadataModel;

            // Slow-path: only single thread should construct metadata model
            lock (_syncObject)
            {
                return _metadataModels.GetOrAdd(path, p =>
                {
                    var assembly = AssemblyDef.Load(p);
                    return BuildAssemblyMembers(assembly);
                });
            }
        }

        private static AssemblyMetadataEntry BuildAssemblyMembers(AssemblyDef assembly)
        {
            var assemblyToken = new AssemblyToken(assembly.MDToken.ToInt32());
            var assemblyMembers = new List<MetadataEntryBase>(capacity: assembly.Modules.Count);
            foreach (var module in assembly.Modules)
            {
                var moduleToken = new ModuleToken(module.MDToken.ToInt32());
                var moduleMembers = BuildModuleMembers(module);
                assemblyMembers.Add(new ModuleMetadataEntry(module.Name, moduleToken, module.Location, moduleMembers.ToImmutableArray()));
            }
            SortEntriesBy(assemblyMembers, m => m.Name);
            return new AssemblyMetadataEntry(assembly.Name, assemblyToken, assembly.FullName, assemblyMembers.ToImmutableArray());
        }

        private static List<MetadataEntryBase> BuildModuleMembers(ModuleDef module)
        {
            var moduleMembers = new List<MetadataEntryBase>(capacity: module.Types.Count);
            foreach (var type in module.GetTypes())
            {
                var typeDefinitionToken = new TypeDefinitionToken(type.MDToken.ToInt32());
                var typeMembers = BuildTypeMembers(type);
                moduleMembers.Add(new TypeMetadataEntry(type.Name, type.FullName, typeDefinitionToken, typeMembers.ToImmutableArray()));
            }
            SortEntriesBy(moduleMembers, m => m.Name);
            return moduleMembers;
        }

        private static List<MetadataEntryBase> BuildTypeMembers(TypeDef type)
        {
            var typeMembers = new List<MetadataEntryBase>(capacity: type.Methods.Count);
            foreach (var method in type.Methods)
            {
                var methodDefinitionToken = new MethodDefinitionToken(method.MDToken.ToInt32());
                typeMembers.Add(new MethodMetadataEntry(method.Name, methodDefinitionToken, ImmutableArray<MetadataEntryBase>.Empty));
            }
            SortEntriesBy(typeMembers, m => m.Name);
            return typeMembers;
        }

        private static void SortEntriesBy<TProperty>(List<MetadataEntryBase> entries, Func<MetadataEntryBase, TProperty> selector)
            where TProperty : IComparable<TProperty>
        {
            entries.Sort((first, second) => selector(first).CompareTo(selector(second)));
        }
    }
}
