using System.Diagnostics;

namespace Meditation.Common.Models
{
    public class ProcessInfo
    {
        private readonly Process processInternal;
        private readonly ProcessType processType;

        public ProcessInfo(Process processInternal, ProcessType processType)
        {
            this.processInternal = processInternal;
            this.processType = processType;
        }

        public int Id => processInternal.Id;
        public string Name => processInternal.ProcessName;
        public ProcessType Type => processType;

        public Process ProcessInternal => processInternal;

        public override string ToString()
            => $"[{Id}] {Name}";
    }
}
