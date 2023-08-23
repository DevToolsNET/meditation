using Avalonia.Controls;
using Meditation.UI.Utilities;
using Meditation.UI.ViewModels;

namespace Meditation.UI.Views
{

    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = this.GetServiceProvider().CreateInstance<MainViewModel>();
        }
    }
}