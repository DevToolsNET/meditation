using dnlib.DotNet;
using Meditation.MetadataLoaderService.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HarmonyLib;
using Meditation.PatchLibrary;

namespace Meditation.MetadataLoaderService.Services
{
    internal class DependencyResolver : IDependencyResolver
    {
        private readonly IMetadataLoaderInternal _metadataLoader;
        private static readonly ImmutableArray<string> _meditationAssemblies;

        static DependencyResolver()
        {
            var builder = ImmutableArray.CreateBuilder<string>();
            builder.Add(typeof(Harmony).Assembly.Location);
            builder.Add(typeof(MeditationPatchAssemblyTargetAttribute).Assembly.Location);
            builder.Add(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll"));
            _meditationAssemblies = builder.ToImmutableArray();
        }

        public DependencyResolver(IMetadataLoaderInternal metadataLoader)
        {
            _metadataLoader = metadataLoader;
            MeditationAssemblies = _meditationAssemblies;
        }

        public ImmutableArray<string> MeditationAssemblies { get; }

        public DirectoryInfo GetReferenceAssembliesFolder(ModuleMetadataEntry coreLibraryModule)
        {
            if (!coreLibraryModule.ModuleDef.IsCoreLibraryModule.HasValue || !coreLibraryModule.ModuleDef.IsCoreLibraryModule.Value)
                throw new ArgumentException($"Expected a core library module, instead received: \"{coreLibraryModule.Path}\".", nameof(coreLibraryModule));

            if (Path.GetDirectoryName(coreLibraryModule.Path) is not { } referenceAssembliesPath)
                throw new NotSupportedException($"Cannot resolve reference assemblies folder based on core library module path: \"{coreLibraryModule.Path}\".");

            return new DirectoryInfo(referenceAssembliesPath);
        }

        public ImmutableArray<string> GetReferencedAssemblies(ModuleMetadataEntry module)
        {
            var builder = new HashSet<string>(comparer: StringComparer.InvariantCulture);
            var resolver = _metadataLoader.AssemblyResolver;
            resolver.FindExactMatch = false;
            resolver.UseGAC = false;
            resolver.PostSearchPaths.Clear();
            resolver.PreSearchPaths.Clear();
            foreach (var path in _metadataLoader.ModulePaths)
                resolver.PreSearchPaths.Add(path);

            foreach (var reference in GetReferencedAssemblies(module.ModuleDef, resolver))
                builder.Add(reference);

            return builder.ToImmutableArray();
        }

        private IEnumerable<string> GetReferencedAssemblies(ModuleDef module, AssemblyResolver resolver)
        {
            foreach (var reference in module.GetAssemblyRefs().Select(ar => resolver.Resolve(ar, module)))
            {
                if (reference == null)
                {
                    // FIXME [#16]: add logging
                    // Unable to resolve referenced assembly
                    continue;
                }

                yield return reference.ManifestModule.Location;

                foreach (var transitiveReference in GetReferencedAssemblies(reference.ManifestModule, resolver))
                    yield return transitiveReference;
            }
        }

        public ModuleMetadataEntry GetCoreLibrary(MethodMetadataEntry method)
        {
            MetadataEntryBase? result;

            var declaringModule = method.MethodDef.Module;
            if (declaringModule.IsCoreLibraryModule.HasValue && declaringModule.IsCoreLibraryModule.Value)
            {
                // Method is defined within a core library
                var path = declaringModule.Location;
                if (!_metadataLoader.TryGetLoadedMetadataFromPath(path, out result))
                    throw new Exception($"Could not resolve metadata model for core library \"{path}\".");
            }
            else
            {
                // Core library is one of the referenced assemblies
                if (!TryGetCoreLibraryMetadataEntry(method.MethodDef, out result))
                    throw new Exception($"Could not resolve core library for module \"{method.ModulePath}\".");
            }

            return ResolveCoreLibraryModule(result);
        }

        private bool TryGetCoreLibraryMetadataEntry(MethodDef methodDef, [NotNullWhen(returnValue: true)] out MetadataEntryBase? coreLibraryMetadataEntry)
        {
            coreLibraryMetadataEntry = null;
            if (methodDef.Module.GetAssemblyRefs().SingleOrDefault(ar => ar.IsCorLib()) is not { } coreLibraryAssemblyRef)
                return false;

            var loadedModules = _metadataLoader.LoadedModules;
            if (loadedModules.SingleOrDefault(md => DoesModuleNameMatchAssemblyName(md.Name, coreLibraryAssemblyRef.Name)) is not { } coreLibraryModule)
                return false;

            return _metadataLoader.TryGetLoadedMetadataFromPath(coreLibraryModule.Location, out coreLibraryMetadataEntry);
        }

        private static ModuleMetadataEntry ResolveCoreLibraryModule(MetadataEntryBase metadataEntry)
        {
            return metadataEntry switch
            {
                ModuleMetadataEntry moduleMetadataEntry => moduleMetadataEntry,
                AssemblyMetadataEntry assemblyMetadataEntry => assemblyMetadataEntry.ManifestModule,
                _ => throw new Exception($"Could not resolve {metadataEntry} to a core library module."),
            };
        }

        private static bool DoesModuleNameMatchAssemblyName(string moduleName, string assemblyName)
        {
            return string.Equals(moduleName, $"{assemblyName}.dll", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
