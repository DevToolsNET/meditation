using System.Runtime.InteropServices;

namespace Meditation.Bootstrap
{
    public static class EntryPoint
    {
        // Note: this method is dynamically invoked by tests
        [UnmanagedCallersOnly(EntryPoint = "MeditationSanityCheck")]
        public static uint SanityCheck()
        {
            return 0xDEAD_C0DE;
        }

        // Note: this method is dynamically invoked by Meditation.InjectorService
        [UnmanagedCallersOnly(EntryPoint = "MeditationInitialize")]
        public static uint Initialize()
        {
            return 0xDEAD_C0DE;
        }
    }
}