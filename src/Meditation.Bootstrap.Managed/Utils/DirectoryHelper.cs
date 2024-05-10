using System.IO;

namespace Meditation.Bootstrap.Managed.Utils
{
    internal static class DirectoryHelper
    {
        public static void EnsureExists(string fullPath)
        {
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
        }
    }
}
