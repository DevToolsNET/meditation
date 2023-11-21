using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Immutable;

namespace Meditation.UI.ViewModels.IDE
{
    public partial class IdeDiagnosticsListViewModel : ViewModelBase
    {
        [ObservableProperty] private ImmutableArray<IdeDiagnosticsViewModel> _diagnostics;
    }
}
