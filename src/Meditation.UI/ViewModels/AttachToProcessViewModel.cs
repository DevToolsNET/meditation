using Meditation.UI.Controllers;

namespace Meditation.UI.ViewModels
{
    public partial class AttachToProcessViewModel : ViewModelBase
    {
        public AttachToProcessController AttachToProcessController { get; }

        public AttachToProcessViewModel(AttachToProcessController attachToProcessController)
        {
            AttachToProcessController = attachToProcessController;
        }
    }
}
