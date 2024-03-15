using CommunityToolkit.Mvvm.ComponentModel;

namespace Meditation.UI.ViewModels.IDE
{
    public partial class IdeDiagnosticsEntryViewModel : ViewModelBase
    {
        [ObservableProperty] private string _severity;
        [ObservableProperty] private string _location;
        [ObservableProperty] private string _code;
        [ObservableProperty] private string _message;

        public IdeDiagnosticsEntryViewModel(string severity, string code, string location, string message)
        {
            Severity = severity;
            Code = code;
            Location = location;
            Message = message;
        }
    }
}
