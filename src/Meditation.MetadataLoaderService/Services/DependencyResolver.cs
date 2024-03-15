using dnlib.DotNet;
using HarmonyLib;
using Meditation.MetadataLoaderService.Models;
using Meditation.PatchLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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

            foreach (var reference in GetReferencedAssemblies(module.ModuleDef, builder, resolver))
                builder.Add(reference);

            return builder.ToImmutableArray();
        }

        private static IEnumerable<string> GetReferencedAssemblies(ModuleDef module, HashSet<string> processedAssemblies, AssemblyResolver resolver)
        {
            foreach (var reference in module.GetAssemblyRefs().Select(ar => resolver.Resolve(ar, module)))
            {
                if (reference == null)
                {
                    // FIXME [#16]: add logging
                    // Unable to resolve referenced assembly
                    continue;
                }

                var assemblyLocation = reference.ManifestModule.Location;
                if (processedAssemblies.Contains(assemblyLocation))
                    continue;

                yield return assemblyLocation;

                foreach (var transitiveReference in GetReferencedAssemblies(reference.ManifestModule, processedAssemblies, resolver))
                    yield return transitiveReference;
            }
        }
    }
}
