using System.Threading;
using System.Threading.Tasks;

namespace Meditation.PatchingService
{
    public interface IPatchStorage
    {
        string GetRootFolderForPatches();

        bool PatchExists(string fileName);

        Task StorePatch(
            string fileName, 
            byte[] data, 
            bool overwriteExistingFile = false,
            CancellationToken ct = default);
    }
}
