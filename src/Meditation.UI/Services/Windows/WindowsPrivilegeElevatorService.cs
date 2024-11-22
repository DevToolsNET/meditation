using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Meditation.UI.Services.Windows
{
    [SupportedOSPlatform("windows")]
    internal class WindowsPrivilegeElevatorService : IPrivilegeElevatorService
    {
        public bool IsElevated()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void RestartAsElevated()
        {
            var processStartInfo = new ProcessStartInfo
            {
                Verb = "runas",
                FileName = Process.GetCurrentProcess().MainModule!.FileName,
                UseShellExecute = true
            };

            Process.Start(processStartInfo);
            Environment.Exit(0);
        }
    }
}
