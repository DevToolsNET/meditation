using Avalonia.Controls;
using Meditation.UI.Utilities;
using Meditation.UI.ViewModels;

namespace Meditation.UI.Views
{
    public partial class ProcessListView : UserControl
    {
        public ProcessListView()
        {
            InitializeComponent();
            DataContext = this.GetServiceProvider().CreateInstance<ProcessListViewModel>();
        }
    }
}
