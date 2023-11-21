using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Meditation.UI.ViewModels.IDE
{
    public partial class IdeDiagnosticsSummaryViewModel : ViewModelBase
    {
        [ObservableProperty] private ObservableCollection<IdeDiagnosticsEntryViewModel> _diagnosticEntries;
        [ObservableProperty] private string _output;

        public IdeDiagnosticsSummaryViewModel()
        {
            _output = string.Empty;
            _diagnosticEntries = new ObservableCollection<IdeDiagnosticsEntryViewModel>();
        }
    }
}
