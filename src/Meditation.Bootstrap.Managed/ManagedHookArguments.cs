using System;
using System.Diagnostics.CodeAnalysis;

namespace Meditation.Bootstrap.Managed
{
    public record ManagedHookArguments(
        string LoggingFileName,
        string UniqueIdentifier,
        string Argument)
    {
        private static readonly char[] SeparatorsForParsing = { '#' };

        public static bool TryParse(string input, out ManagedHookErrorCode error, [NotNullWhen(true)] out ManagedHookArguments? hookArgs)
        {
            hookArgs = null;

            // Ensure there are enough elements
            var tokens = input.Split(SeparatorsForParsing, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3)
            {
                error = ManagedHookErrorCode.InvalidArguments_HookArgs_CouldNotParse;
                return false;
            }

            hookArgs = new ManagedHookArguments(
                LoggingFileName: tokens[0],
                UniqueIdentifier: tokens[1],
                Argument: tokens[2]);

            error = ManagedHookErrorCode.Ok;
            return true;
        }
    }
}
