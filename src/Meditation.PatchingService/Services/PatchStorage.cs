using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.PatchingService.Services
{
    internal class PatchStorage : IPatchStorage
    {
        internal const string MeditationFolder = "Meditation";
        internal static readonly string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        internal static readonly string RootFolder = Path.Combine(AppDataFolder, MeditationFolder);

        public string GetRootFolderForPatches() => RootFolder;

        public PatchStorage()
        {
            if (!Directory.Exists(RootFolder))
                Directory.CreateDirectory(RootFolder);
        }

        public bool PatchExists(string fileName)
        {
            var fullFileName = Path.Combine(RootFolder, fileName);
            return File.Exists(fullFileName);
        }

        public Task StorePatch(
            string fileName, 
            byte[] data, 
            bool overwriteExistingFile = false,
            CancellationToken ct = default)
        {
            if (PatchExists(fileName))
            {
                if (!overwriteExistingFile)
                    throw new IOException($"File {fileName} already exists.");
                File.Delete(fileName);
            }

            return File.WriteAllBytesAsync(fileName, data, ct);
        }
    }
}
