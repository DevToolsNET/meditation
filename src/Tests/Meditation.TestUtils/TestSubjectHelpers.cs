using System;
using CliWrap;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.TestUtils
{
    public static class TestSubjectHelpers
    {
        public static TestSubjectController GetTestSubjectExecutionController(string netSdkIdentifier)
        {
            var isNetFramework = netSdkIdentifier.StartsWith("net4");
            var projectSuffix = isNetFramework ? "NetFramework" : "NetCore";
            var extension = isNetFramework ? "exe" : "dll";
            var assemblyPath = Path.GetFullPath(
                Path.Combine(
                    "../../../../",
                    $"Meditation.TestSubject.{projectSuffix}",
                    "bin",
                    "Debug",
                    netSdkIdentifier,
                    $"Meditation.TestSubject.{projectSuffix}.{extension}"));
            var executable = isNetFramework ? assemblyPath : "dotnet";
            var argument = isNetFramework ? string.Empty : assemblyPath;
            var cts = new CancellationTokenSource();

            return new TestSubjectController(
                command: Cli.Wrap(executable)
                    .WithArguments(argument),
                startAction: cmd => cmd.ExecuteAsync(cts.Token),
                disposeAction: () => cts.Cancel());
        }

        public static async Task KillTestSubject(CommandTask<CommandResult> execution)
        {
            try
            {
                await execution;
            }
            catch (OperationCanceledException)
            {
                /* Empty on purpose */
            }
        }
    }
}
