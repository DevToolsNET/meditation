using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Meditation.UI.ViewModels.IDE
{
    public partial class IdeStatusBarViewModel : ViewModelBase
    {
        [ObservableProperty] private string? _text;
        [ObservableProperty] private IBrush _backgroundBrush;
        private static readonly IBrush _defaultBrush = Brushes.LightGray;
        private const string DefaultTitle = "Ready";

        public IdeStatusBarViewModel()
        {
            _text = DefaultTitle;
            _backgroundBrush = _defaultBrush;
        }

        public void SetSuccessStatus(string text)
        {
            Text = text;
            BackgroundBrush = Brushes.LightGreen;
        }

        public void SetWarningStatus(string text)
        {
            Text = text;
            BackgroundBrush = Brushes.Yellow;
        }

        public void SetErrorStatus(string text)
        {
            Text = text;
            BackgroundBrush = Brushes.Orange;
        }

        public void SetInformationStatus(string text)
        {
            Text = text;
            BackgroundBrush = _defaultBrush;
        }
    }
}
