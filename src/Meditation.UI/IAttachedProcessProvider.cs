using System;
using Meditation.AttachProcessService;

namespace Meditation.UI
{
    public interface IAttachedProcessProvider
    {
        event Action<IProcessSnapshot?>? AttachedProcessChanged;

        IProcessSnapshot? ProcessSnapshot { get; }
    }
}
