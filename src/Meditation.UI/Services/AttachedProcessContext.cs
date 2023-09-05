using Meditation.AttachProcessService;
using System;

namespace Meditation.UI.Services
{
    internal class AttachedProcessContext : IAttachedProcessProvider, IAttachedProcessController
    {
        public event Action<IProcessSnapshot?>? AttachedProcessChanged;

        public IProcessSnapshot? ProcessSnapshot { get; private set; }

        public void Attach(IProcessSnapshot processSnapshot)
        {
            ProcessSnapshot = processSnapshot;
            AttachedProcessChanged?.Invoke(processSnapshot);
        }

        public void Detach()
        {
            ProcessSnapshot = null;
            AttachedProcessChanged?.Invoke(null);
        }
    }
}
