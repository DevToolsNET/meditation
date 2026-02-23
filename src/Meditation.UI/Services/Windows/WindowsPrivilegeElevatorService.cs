using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Threading.Tasks;

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

        public Task RestartAsElevated()
        {
            var processStartInfo = new ProcessStartInfo
            {
                Verb = "runas",
                FileName = Environment.ProcessPath,
                UseShellExecute = true
            };

            Process.Start(processStartInfo);
            Environment.Exit(0);
            return Task.CompletedTask;
        }
    }
}
