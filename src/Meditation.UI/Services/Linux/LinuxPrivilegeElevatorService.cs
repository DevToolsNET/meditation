using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Meditation.UI.Controllers.Utils;

namespace Meditation.UI.Services.Linux;

[SupportedOSPlatform("linux")]
internal sealed partial class LinuxPrivilegeElevatorService : IPrivilegeElevatorService
{
    public bool IsElevated()
    {
        return NativeBindings.geteuid() == 0;
    }

    public Task RestartAsElevated()
    {
        return DialogUtilities.ShowMessageBox(
            title: "Unsupported operation",
            content: "Restarting the application with elevated privileges is not supported on Linux.");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Matches native library function names")]
    private static partial class NativeBindings
    {
        [LibraryImport("libc")]
        public static partial uint geteuid();
    }
}