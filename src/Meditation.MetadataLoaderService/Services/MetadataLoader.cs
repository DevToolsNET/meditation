using dnlib.DotNet;
using Meditation.MetadataLoaderService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Meditation.MetadataLoaderService.Services
{
    internal class MetadataLoader : IMetadataLoaderInternal
    {
        private readonly ConcurrentDictionary<AssemblyName, string> _assemblyPaths;
        private readonly ConcurrentDictionary<string, ModuleDef> _modules;
        private readonly ConcurrentDictionary<string, MetadataEntryBase> _metadataModels;
        private readonly ModuleContext _moduleContext;
        private readonly AssemblyResolver _assemblyResolver;
        private readonly object _syncObject;
        private ImmutableArray<string> _modulePaths;

        public ImmutableArray<string> ModulePaths => _modulePaths;
        public ICollection<ModuleDef> LoadedModules => _modules.Values;
        public AssemblyResolver AssemblyResolver => _assemblyResolver;

        public MetadataLoader()
        {
            _assemblyPaths = new ConcurrentDictionary<AssemblyName, string>(new AssemblyNameEqualityComparer());
            _modules = new ConcurrentDictionary<string, ModuleDef>();
            _metadataModels = new ConcurrentDictionary<string, MetadataEntryBase>();
            _moduleContext = new ModuleContext() { };
            _assemblyResolver = new AssemblyResolver(_moduleContext)
            {
                EnableFrameworkRedirect = true,
                FindExactMatch = false,
                UseGAC = false
            };
            _moduleContext.AssemblyResolver = _assemblyResolver;
            _modulePaths = ImmutableArray<string>.Empty;
            ;
            _syncObject = new object();
        }

        public bool TryGetLoadedMetadataFromPath(string path, [NotNullWhen(true)] out MetadataEntryBase? metadata)
            => _metadataModels.TryGetValue(path, out metadata);

        public IEnumerable<MetadataEntryBase> LoadMetadataFromProcess(IEnumerable<string> modulePaths)
        {
            var assembliesLookup = new Dictionary<(UTF8String Name, Version Version, UTF8String Culture, PublicKey PublicKey), AssemblyDef>();
            var netModulesLookup = new Dictionary<Guid, ModuleDef>();
            foreach (var modulePath in modulePaths.OrderBy(Path.GetFileName))
            {
                var (metadata, moduleDef) = LoadMetadataImpl(modulePath);
                if (moduleDef.Assembly != null)
                {
                    // Loading a regular assembly
                    var assemblyDef = moduleDef.Assembly;
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
                else
                {
                    // Loading a netmodule
                    // Note: all compliant modules have Mvid (module identifier) unless it was manually edited
                    // Reference: https://learn.microsoft.com/en-us/dotnet/api/system.reflection.module.moduleversionid?view=net-7.0
                    var moduleIdentifier = moduleDef.Mvid ?? default;
                    if (netModulesLookup.ContainsKey(moduleIdentifier))
                    {
                        // Identical netmodule was loaded multiple times
                        continue;
                    }

                    netModulesLookup.Add(moduleIdentifier, moduleDef);
                    yield return metadata;
                }
            }
        }

        public MetadataEntryBase LoadMetadataFromPath(string path) => LoadMetadataImpl(path).MetadataModel;

        private (MetadataEntryBase MetadataModel, ModuleDef Module) LoadMetadataImpl(string path)
        {
            // Fast-path: metadata model is already constructed
            if (_modules.TryGetValue(path, out var moduleDef) && _metadataModels.TryGetValue(path, out var metadataModel))
                return (metadataModel, moduleDef);

            // Slow-path: only single thread should construct metadata model
            lock (_syncObject)
            {
                var directory = Path.GetDirectoryName(path);
                if (directory != null && !_modulePaths.Contains(directory))
                    _modulePaths = _modulePaths.Add(directory);

                moduleDef = _modules.GetOrAdd(path, _ => ModuleDefMD.Load(path, _moduleContext));
                metadataModel = _metadataModels.GetOrAdd(path, _ =>
                {
                    if (moduleDef.Assembly != null)
                    {
                        var assemblyName = new AssemblyName(moduleDef.Assembly.FullName);
                        _assemblyPaths.TryAdd(assemblyName, path);
                        return BuildAssemblyMetadata(moduleDef.Assembly);
                    }

                    return BuildModuleMetadata(moduleDef);
                });

                return (metadataModel, moduleDef);
            }
        }

        public MetadataEntryBase LoadMetadataFromAssemblyName(AssemblyName assemblyName)
        {
            if (_assemblyPaths.TryGetValue(assemblyName, out var assemblyPath))
                return LoadMetadataFromPath(assemblyPath);

            throw new FileNotFoundException($"Could not find {assemblyName}.");
        }

        private static AssemblyMetadataEntry BuildAssemblyMetadata(AssemblyDef assembly)
        {
            var assemblyMembers = BuildAssemblyMembers(assembly);
            return new AssemblyMetadataEntry(assembly.Name, assembly.FullName, assembly.Version, assemblyMembers.ToImmutableArray());
        }

        private static List<MetadataEntryBase> BuildAssemblyMembers(AssemblyDef assembly)
        {
            var assemblyMembers = new List<MetadataEntryBase>(capacity: assembly.Modules.Count);
            foreach (var module in assembly.Modules)
            {
                var moduleMembers = BuildModuleMembers(module);
                assemblyMembers.Add(new ModuleMetadataEntry(module, moduleMembers.ToImmutableArray()));
            }
            SortEntriesBy(assemblyMembers, m => m.Name);
            return assemblyMembers;
        }

        private static ModuleMetadataEntry BuildModuleMetadata(ModuleDef module)
        {
            var moduleMembers = BuildModuleMembers(module);
            return new ModuleMetadataEntry(module, moduleMembers.ToImmutableArray());
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
