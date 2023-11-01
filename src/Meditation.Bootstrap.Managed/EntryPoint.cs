using System.Diagnostics;

namespace Meditation.Bootstrap.Managed
{
    public class EntryPoint
    {
        // Note: this method is dynamically invoked
        public static int Hook(string argument)
        {
            Process.Start(argument);
            return 0;
        }
    }
}