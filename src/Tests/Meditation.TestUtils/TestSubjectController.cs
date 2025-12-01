using System;
using CliWrap;

namespace Meditation.TestUtils
{
    public class TestSubjectController : IDisposable
    {
        private readonly Command _command;
        private readonly Func<Command, CommandTask<CommandResult>> _starter;
        private readonly Action _terminator;
        private bool _disposed;

        public TestSubjectController(Command command, Func<Command, CommandTask<CommandResult>> startAction, Action disposeAction)
        {
            _command = command;
            _starter = startAction;
            _terminator = disposeAction;
        }

        public CommandTask<CommandResult> ExecuteAsync()
        {
            return _starter(_command);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _terminator();
        }
    }
}
