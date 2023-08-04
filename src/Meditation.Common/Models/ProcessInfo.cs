using System.Diagnostics;

namespace Meditation.Common.Models
{
    public class ProcessInfo
    {
        private readonly Process processInternal;

        public ProcessInfo(Process processInternal)
        {
            this.processInternal = processInternal;
        }

        public int Id => processInternal.Id;
        public string Name => processInternal.ProcessName;

        public override string ToString()
            => $"[{Id}] {Name}";
    }
}
