using System;

namespace Meditation.UI.Configuration
{
    public class LoggingConfiguration
    {
        public string MainApplicationFileName { get; }
        public string BootstrapNativeFileName { get; }
        public string BootstrapManagedFileName { get; }

        public LoggingConfiguration(
            string mainApplicationFileName,
            string bootstrapNativeFileName,
            string bootstrapManagedFileName)
        {
            MainApplicationFileName = Environment.ExpandEnvironmentVariables(mainApplicationFileName);
            BootstrapNativeFileName = Environment.ExpandEnvironmentVariables(bootstrapNativeFileName);
            BootstrapManagedFileName = Environment.ExpandEnvironmentVariables(bootstrapManagedFileName);
        }
    }
}
