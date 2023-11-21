using CommunityToolkit.Mvvm.ComponentModel;

namespace Meditation.UI.ViewModels.IDE
{
    public partial class IdeDiagnosticsViewModel : ViewModelBase
    {
        [ObservableProperty] private string _severity;
        [ObservableProperty] private int _line;
        [ObservableProperty] private string _code;
        [ObservableProperty] private string _message;

        public IdeDiagnosticsViewModel(string severity, string code, int line, string message)
        {
            Severity = severity;
            Code = code;
            Line = line;
            Message = message;
        }
    }
}
