using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Meditation.InjectorService
{
    public interface IProcessInjector
    {
        Task<SafeHandle> TryInjectModule(int pid, string assemblyPath);
    }
}
