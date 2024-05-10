using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Meditation.UI.Services.Dialogs;
using System.Threading.Tasks;

namespace Meditation.UI
{
    public interface IAvaloniaDialogService
    {
        DialogLifetime CreateDialog<TWindow>() where TWindow : Window;

        Task<IStorageFile?> ShowSaveFileDialog(string title, string extension, string folder, string fileName);
    }
}