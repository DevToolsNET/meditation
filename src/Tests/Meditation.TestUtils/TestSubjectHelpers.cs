using CliWrap;
using System.IO;

namespace Meditation.TestUtils
{
    public static class TestSubjectHelpers
    {
        public static Command GetTestSubjectExecutionCommand(string netSdkIdentifier)
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
            return Cli.Wrap(executable).WithArguments(argument);
        }
    }
}
