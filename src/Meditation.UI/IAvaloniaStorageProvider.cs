using Avalonia.Platform.Storage;

namespace Meditation.UI
{
    public interface IAvaloniaStorageProvider
    {
        IStorageProvider GetStorageProvider();
    }
}
