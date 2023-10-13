using Avalonia.Controls;
using Meditation.UI.Services.Dialogs;

namespace Meditation.UI
{
    public interface IAvaloniaDialogService
    {
        DialogLifetime CreateDialog<TWindow>() where TWindow : Window;
    }
}