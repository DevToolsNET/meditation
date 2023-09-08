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

        public MetadataLoader()
        {
            _loadedAssemblies = new ConcurrentDictionary<string, AssemblyDef>();
            _metadataModels = new ConcurrentDictionary<string, AssemblyMetadataEntry>();
        }

        public AssemblyMetadataEntry LoadMetadataFromAssembly(string path)
        {
            return _metadataModels.GetOrAdd(path, p =>
            {
                var assembly = LoadAssembly(p);
                return BuildAssemblyMembers(assembly);
            });
        }

        private AssemblyDef LoadAssembly(string path)
        {
            try
            {
                return _loadedAssemblies.GetOrAdd(path, p => AssemblyDef.Load(path));
            }
            catch (Exception)
            {
                // FIXME: add logging
                throw;
            }
        }

        private static AssemblyMetadataEntry BuildAssemblyMembers(AssemblyDef assembly)
        {
            var assemblyMembers = new List<MetadataEntryBase>();
            foreach (var module in assembly.Modules)
            {
                var moduleMembers = BuildModuleMembers(module);
                assemblyMembers.Add(new ModuleMetadataEntry(module.Name, module.MDToken.ToInt32(), module.Location, moduleMembers.ToImmutableArray()));
            }
            SortEntriesBy(assemblyMembers, m => m.Name);
            return new AssemblyMetadataEntry(assembly.Name, assembly.MDToken.ToInt32(), assembly.FullName, assemblyMembers.ToImmutableArray());
        }

        private static List<MetadataEntryBase> BuildModuleMembers(ModuleDef module)
        {
            var moduleMembers = new List<MetadataEntryBase>();
            foreach (var type in module.GetTypes())
            {
                var typeMembers = BuildTypeMembers(type);
                moduleMembers.Add(new TypeMetadataEntry(type.Name, type.FullName, type.MDToken.ToInt32(), typeMembers.ToImmutableArray()));
            }
            SortEntriesBy(moduleMembers, m => m.Name);
            return moduleMembers;
        }

        private static List<MetadataEntryBase> BuildTypeMembers(TypeDef type)
        {
            var typeMembers = new List<MetadataEntryBase>(capacity: type.Methods.Count);
            foreach (var method in type.Methods)
                typeMembers.Add(new MethodMetadataEntry(method.Name, method.MDToken.ToInt32(), ImmutableArray<MetadataEntryBase>.Empty));
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
