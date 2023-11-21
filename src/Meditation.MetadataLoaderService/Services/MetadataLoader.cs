using dnlib.DotNet;
using Meditation.MetadataLoaderService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Meditation.MetadataLoaderService.Services
{
    internal class MetadataLoader : IMetadataLoader
    {
        private readonly ConcurrentDictionary<string, AssemblyDef> _assemblies;
        private readonly ConcurrentDictionary<string, AssemblyMetadataEntry> _metadataModels;
        private readonly object _syncObject;

        public MetadataLoader()
        {
            _assemblies = new ConcurrentDictionary<string, AssemblyDef>();
            _metadataModels = new ConcurrentDictionary<string, AssemblyMetadataEntry>();
            _syncObject = new object();
        }

        public ModuleMetadataEntry GetCoreLibrary(MethodMetadataEntry method)
        {
            var coreLibRef = method.MethodDef.Module.GetAssemblyRefs().SingleOrDefault(ar => ar.IsCorLib());
            if (coreLibRef == null)
                throw new NotSupportedException($"Could not determine core library for module \"{method.ModulePath}\".");

            var coreLibDef = _assemblies.Values.SingleOrDefault(ad => ad.Name == coreLibRef.Name);
            if (coreLibDef == null || !_metadataModels.TryGetValue(coreLibDef.ManifestModule.Location, out var metadataEntry))
                throw new InvalidOperationException("Core library has not been loaded yet.");

            return metadataEntry.ManifestModule;
        }

        public IEnumerable<AssemblyMetadataEntry> LoadMetadataFromProcess(IEnumerable<string> modulePaths)
        {
            var assembliesLookup = new Dictionary<(UTF8String Name, Version Version, UTF8String Culture, PublicKey PublicKey), AssemblyDef>();
            foreach (var modulePath in modulePaths.OrderBy(Path.GetFileName))
            {
                var (metadata, assemblyDef) = LoadMetadataFromAssemblyImpl(modulePath);
                if (assembliesLookup.ContainsKey((assemblyDef.Name, assemblyDef.Version, assemblyDef.Culture, assemblyDef.PublicKey)))
                {
                    // This means that the assembly is already processed
                    // It happens usually in connection with NGEN (see: https://learn.microsoft.com/en-us/dotnet/framework/tools/ngen-exe-native-image-generator).
                    // For example, consider case when we are processing modules: System.Data.dll and subsequently try to process System.Data.ni.dll

                    // FIXME [#16]: log reason for skipping assembly processing
                    continue;
                }

                assembliesLookup.Add((assemblyDef.Name, assemblyDef.Version, assemblyDef.Culture, assemblyDef.PublicKey), assemblyDef);
                yield return metadata;
            }

        }

        public AssemblyMetadataEntry LoadMetadataFromAssembly(string path) => LoadMetadataFromAssemblyImpl(path).MetadataModel;

        private (AssemblyMetadataEntry MetadataModel, AssemblyDef Assembly) LoadMetadataFromAssemblyImpl(string path)
        {
            // Fast-path: metadata model is already constructed
            if (_assemblies.TryGetValue(path, out var assemblyDef) && _metadataModels.TryGetValue(path, out var metadataModel))
                return (metadataModel, assemblyDef);

            // Slow-path: only single thread should construct metadata model
            lock (_syncObject)
            {
                assemblyDef = _assemblies.GetOrAdd(path, _ => AssemblyDef.Load(path));
                metadataModel = _metadataModels.GetOrAdd(path, _ => BuildAssemblyMembers(assemblyDef));
                return (metadataModel, assemblyDef);
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
            return new AssemblyMetadataEntry(assembly, assemblyMembers.ToImmutableArray());
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
                typeMembers.Add(new MethodMetadataEntry(method, ImmutableArray<MetadataEntryBase>.Empty));

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
