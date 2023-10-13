using System;

namespace Meditation.AttachProcessService.Models
{
    public readonly record struct ProcessId
    {
        public int Value { get; }

        public ProcessId(int pid)
        {
            if (pid < 0)
                throw new ArgumentOutOfRangeException(nameof(pid), "Value must be non-negative.");

            Value = pid;
        }
    }
}
