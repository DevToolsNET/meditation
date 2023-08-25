using Avalonia.Controls;
using Meditation.UI.Utilities;
using Meditation.UI.ViewModels;

namespace Meditation.UI.Views
{
    public partial class MetadataBrowserView : UserControl
    {
        public MetadataBrowserView()
        {
            InitializeComponent();
            DataContext = this.GetServiceProvider().CreateInstance<MetadataBrowserViewModel>();
        }
    }
}
