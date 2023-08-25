using System;

namespace Meditation.UI.Services
{
    internal class UserInterfaceEventsHub : IUserInterfaceEventsConsumer, IUserInterfaceEventsRaiser
    {
        public event Action<string>? AssemblyLoadRequested;

        public void RaiseAssemblyLoadRequested(string path) => AssemblyLoadRequested?.Invoke(path);
    }
}
