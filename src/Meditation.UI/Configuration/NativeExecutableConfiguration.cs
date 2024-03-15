using System.Runtime.InteropServices;

namespace Meditation.UI.Configuration
{
    public record NativeExecutableConfiguration(
        string Path,
        Architecture Architecture);
}
