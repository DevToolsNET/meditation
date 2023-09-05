using Meditation.AttachProcessService;

namespace Meditation.UI
{
    public interface IAttachedProcessController
    {
        void Attach(IProcessSnapshot processSnapshot);
        void Detach();
    }
}
