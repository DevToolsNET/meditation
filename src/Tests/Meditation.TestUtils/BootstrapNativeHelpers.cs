using System.IO;
using System.Runtime.InteropServices;
using System;

namespace Meditation.TestUtils
{
    public static class BootstrapNativeHelpers
    {
        public static string GetMeditationNativeModulePath()
        {
            const string netSdkIdentifier = "net8.0";
            string runtimeIdentifier;
            string moduleExtension;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                runtimeIdentifier = "win-x64";
                moduleExtension = "dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            var path = Path.GetFullPath(
                Path.Combine(
                    "../../../../../",
                    "Meditation.Bootstrap.Native",
                    "bin",
                    "Debug",
                    netSdkIdentifier,
                    runtimeIdentifier,
                    "publish",
                    $"Meditation.Bootstrap.Native.{moduleExtension}"));

            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            return path;
        }

    }
}
