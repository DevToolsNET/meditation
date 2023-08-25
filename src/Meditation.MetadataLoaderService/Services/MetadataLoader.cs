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
                return BuildMetadataModel(assembly);
            });
        }

        private AssemblyMetadataEntry BuildMetadataModel(AssemblyDef assembly)
        {
            var assemblyMembers = new List<MetadataEntryBase>();
            foreach (var module in assembly.Modules)
            {
                var moduleMembers = new List<MetadataEntryBase>();
                foreach (var type in module.GetTypes())
                {
                    var typeMembers = new List<MetadataEntryBase>();
                    foreach (var method in type.Methods)
                        typeMembers.Add(new MethodMetadataEntry(method.Name, ImmutableArray<MetadataEntryBase>.Empty));
                    moduleMembers.Add(new TypeMetadataEntry(type.Name, type.FullName, typeMembers.ToImmutableArray()));
                }
                assemblyMembers.Add(new ModuleMetadataEntry(module.Name, moduleMembers.ToImmutableArray()));
            }

            return new AssemblyMetadataEntry(assembly.Name, assembly.FullName, assemblyMembers.ToImmutableArray());
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
    }
}
