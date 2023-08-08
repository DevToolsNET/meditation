using System.Diagnostics;

namespace Meditation.Common.Models
{
    public class ProcessInfo
    {
        private readonly Process processInternal;
        private readonly bool? isNetCoreApp;
        private readonly bool? isNetFramework;

        public ProcessInfo(Process processInternal, bool? isNetCoreApp, bool? isNetFramework)
        {
            this.processInternal = processInternal;
            this.isNetCoreApp = isNetCoreApp;
            this.isNetFramework = isNetFramework;
        }

        public int Id => processInternal.Id;
        public string Name => processInternal.ProcessName;
        public string Type => (isNetCoreApp.HasValue && isNetCoreApp.Value) ? "NetCoreApp" : 
                              (isNetFramework.HasValue && isNetFramework.Value) ? "NetFramework" 
                              : "Unknown";

        public Process ProcessInternal => processInternal;

        public override string ToString()
            => $"[{Id}] {Name}";
    }
}
