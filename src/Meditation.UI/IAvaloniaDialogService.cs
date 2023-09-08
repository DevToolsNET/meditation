using Avalonia.Controls;

namespace Meditation.UI
{
    public interface IAvaloniaDialogService
    {
        void DisplayDialog<TWindow>() where TWindow : Window;
        void CloseDialog<TWindow>() where TWindow : Window;
    }
}