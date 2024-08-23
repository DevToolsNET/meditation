using Avalonia.Controls;
using Meditation.UI.Utilities;
using Meditation.UI.ViewModels;

namespace Meditation.UI.Views
{
    public partial class PatchesBrowserView : UserControl
    {
        public PatchesBrowserView()
        {
            InitializeComponent();
            DataContext = this.GetServiceProvider().CreateInstance<PatchesBrowserViewModel>();
        }
    }
}
