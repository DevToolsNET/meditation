using System;

namespace Meditation.UI
{
    public interface IUserInterfaceEventsConsumer
    {
        event Action<string>? AssemblyLoadRequested;
    }
}
