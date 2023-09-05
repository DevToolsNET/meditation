using Avalonia.Controls;
using System.Threading.Tasks;

namespace Meditation.UI
{
    public interface IAvaloniaDialogsContext
    {
        void DisplayDialog<TWindow>() where TWindow : Window;
        Task CloseDialogAsync<TWindow>() where TWindow : Window;
    }
}