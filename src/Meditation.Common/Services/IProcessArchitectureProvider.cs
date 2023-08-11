using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Meditation.Common.Services
{
    public interface IProcessArchitectureProvider
    {
        bool TryGetProcessArchitecture(Process process, [NotNullWhen(returnValue: true)] out Architecture? architecture);
    }
}
