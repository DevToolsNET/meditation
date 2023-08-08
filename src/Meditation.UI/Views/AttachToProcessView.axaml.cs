using Avalonia.Controls;
using Meditation.UI.Utilities;
using Meditation.UI.ViewModels;

namespace Meditation.UI.Views
{
    public partial class AttachToProcessView : UserControl
    {
        public AttachToProcessView()
        {
            InitializeComponent();
            DataContext = this.GetServiceProvider().CreateInstance<AttachToProcessViewModel>();
        }
    }
}
