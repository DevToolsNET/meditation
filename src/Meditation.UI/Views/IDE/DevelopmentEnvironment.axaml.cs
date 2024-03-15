using Avalonia.Controls;
using Meditation.UI.Utilities;
using Meditation.UI.ViewModels.IDE;

namespace Meditation.UI.Views.IDE
{
    public partial class DevelopmentEnvironmentView : UserControl
    {
        public DevelopmentEnvironmentView()
        {
            InitializeComponent();
            DataContext = this.GetServiceProvider().CreateInstance<DevelopmentEnvironmentViewModel>();
        }
    }
}
