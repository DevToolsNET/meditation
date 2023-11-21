using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meditation.PatchLibrary;

namespace Meditation.CompilationService.Services
{
    internal class CompilationService : ICompilationService, IDisposable
    {
        private readonly Dictionary<ProjectId, string> _assemblyNames;
        private readonly Dictionary<string, EmitResult> _emitResults;
        private readonly Dictionary<string, byte[]> _assemblies;
        private readonly AdhocWorkspace _workspace;
        private DocumentId? _documentId;
        private bool _disposed;

        public CompilationService()
        {
            _workspace = new AdhocWorkspace();
            _emitResults = new Dictionary<string, EmitResult>();
            _assemblies = new Dictionary<string, byte[]>();
            _assemblyNames = new Dictionary<ProjectId, string>();
        }

        public ProjectId AddProject(string projectName, string assemblyName, string coreLibraryPath, ImmutableArray<string> additionalReferencedAssemblyPaths)
        {
            var patchLibraryPath = typeof(MeditationPatchAssemblyTargetAttribute).Assembly.Location;
            var harmonyLibraryPath = typeof(HarmonyLib.Harmony).Assembly.Location;
            additionalReferencedAssemblyPaths = additionalReferencedAssemblyPaths.AddRange(patchLibraryPath, harmonyLibraryPath);

            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(
                id: projectId,
                version: versionStamp,
                name: projectName,
                assemblyName: projectName,
                language: LanguageNames.CSharp,
                compilationOptions: new CSharpCompilationOptions(
                    outputKind: OutputKind.DynamicallyLinkedLibrary,
                    platform: Platform.AnyCpu),
                metadataReferences: CreateProjectReferences(coreLibraryPath, additionalReferencedAssemblyPaths));
            _assemblyNames.Add(projectId, assemblyName);
            _workspace.AddProject(projectInfo);
            return projectId;
        }

        public DocumentId AddDocument(ProjectId projectId, string sourceText, Encoding? encoding = null)
        {
            if (_documentId != null)
            {
                // This is a recompilation - remove previous document
                _workspace.TryApplyChanges(_workspace.CurrentSolution.RemoveDocument(_documentId));
            }

            const string documentName = "Patch.cs";
            var sourceCode = SourceText.From(sourceText, encoding);
            var document = _workspace.AddDocument(
                projectId: projectId, 
                name: documentName,
                text: sourceCode);
            _documentId = document.Id;
            return _documentId;
        }

        public byte[] GetProjectAssembly(ProjectId projectId)
        {
            if (!_assemblyNames.TryGetValue(projectId, out var projectName))
                throw new InvalidOperationException($"Project with id {projectId.Id} is unknown.");

            if (!_assemblies.TryGetValue(projectName, out var rawAssemblyData))
                throw new InvalidOperationException($"Project {projectName} must be first successfully built.");

            return rawAssemblyData;
        }

        public async Task<CompilationResult> Build(CancellationToken ct)
        {
            _emitResults.Clear();
            _assemblies.Clear();
            var graph = _workspace.CurrentSolution.GetProjectDependencyGraph();
            var outputLogBuilder = new StringBuilder();
            outputLogBuilder.AppendLine("Beginning build.");
            foreach (var project in graph.GetTopologicallySortedProjects(ct)
                                           .Select(_workspace.CurrentSolution.GetProject)
                                           .Where(project => project != null))
            {
                var projectCompilation = await project!.GetCompilationAsync(ct);

                if (projectCompilation == null)
                {
                    // FIXME [#16]: add logging
                    // Project does not support compilation (invalid project type)
                    continue;
                }

                using var memoryStream = new MemoryStream();
                var assemblyName = _assemblyNames[project.Id];

                outputLogBuilder.AppendLine($"Starting build of project {project!.Name}.");
                var results = projectCompilation.Emit(memoryStream);
                _emitResults.Add(assemblyName, results);
                foreach (var diagnostic in results.Diagnostics)
                    outputLogBuilder.AppendLine($"> {diagnostic.Severity.ToString()}: {diagnostic.GetMessage()}");

                if (!results.Success)
                {
                    // FIXME [#16]: add logging
                    // There were errors during build
                    outputLogBuilder.AppendLine($"Failed to compile project {project!.Name} due to previous issues.");
                    continue;
                }

                outputLogBuilder.AppendLine($"Built project \"{project!.Name}\" without errors.");
                _assemblies.Add(assemblyName, memoryStream.ToArray());
            }

            var success = _emitResults.Count == _assemblies.Count;
            outputLogBuilder.AppendLine();
            outputLogBuilder.AppendLine(success
                ? $"Build succeeded ({_emitResults.Count} projects)."
                : $"Build failed ({_assemblies.Count} successful and {_assemblyNames.Count - _assemblies.Count} failing projects).");
            
            var result = new CompilationResult(
                Success: success, 
                OutputLog: outputLogBuilder.ToString(),
                Assemblies: _assemblies.ToImmutableDictionary(),
                Result: _emitResults.ToImmutableDictionary());
            return result;
        }

        private static ImmutableArray<MetadataReference> CreateProjectReferences(string coreLibraryPath, ImmutableArray<string> additionalReferences)
        {
            var builder = ImmutableArray.CreateBuilder<MetadataReference>();
            builder.Add(MetadataReference.CreateFromFile(coreLibraryPath));

            var coreLibraryModuleName = Path.GetFileNameWithoutExtension(coreLibraryPath);
            var sdkFolder = Path.GetDirectoryName(coreLibraryPath)!;

            builder.Add(MetadataReference.CreateFromFile(Path.Combine(sdkFolder, "netstandard.dll")));
            if (coreLibraryModuleName.Equals("System.Runtime", StringComparison.OrdinalIgnoreCase))
            {
                // Set additional .NET Core references
                builder.Add(MetadataReference.CreateFromFile(Path.Combine(sdkFolder, "System.Private.CoreLib.dll")));
                builder.Add(MetadataReference.CreateFromFile(Path.Combine(sdkFolder, "System.Runtime.dll")));
            }
            else if (coreLibraryModuleName.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
            {
                // Set additional .NET Framework references
                builder.Add(MetadataReference.CreateFromFile(Path.Combine(sdkFolder, "mscorlib.dll")));
            }
            else
            {
                // Unknown implementation, try to advance and hope that it works
                // FIXME [#16]: logging
            }

            foreach (var item in additionalReferences)
                builder.Add(MetadataReference.CreateFromFile(item));
            return builder.ToImmutableArray();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _workspace.Dispose();
            _disposed = true;
        }
    }
}
