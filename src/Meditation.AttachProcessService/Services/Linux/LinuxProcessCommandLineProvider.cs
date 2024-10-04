using CliWrap;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService.Services.Linux
{
    internal class LinuxProcessCommandLineProvider : IProcessCommandLineProvider
    {
        public async Task<string?> TryGetCommandLineArgumentsAsync(Process process, CancellationToken ct)
        {
            var stdoutBuffer = new StringBuilder();
            var stderrBuffer = new StringBuilder();
            var command = Cli.Wrap("/bin/sh")
                .WithArguments(["-c", $"ps -q {process.Id} -o args --no-headers"])
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdoutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderrBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync(ct);

            var result = await command;
            var stdout = stdoutBuffer.ToString();
            var stderr = stderrBuffer.ToString();
            if (!result.IsSuccess || stderr.Length != 0)
            {
                // FIXME [#16]: add logging for stderr and error code of process
                return null;
            }

            return stdout;
        }
    }
}
