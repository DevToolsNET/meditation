using System;

namespace Meditation.UI.Configuration
{
    public record ApplicationConfiguration(
        string UniquePatchingIdentifierScope,
        NativeExecutableConfiguration[] NativeExecutables,
        LoggingConfiguration Logging)
    {
        public void Validate()
        {
            if (NativeExecutables.Length == 0)
                throw new ArgumentException(CreateEmptyArrayMessage(settingScope: null, settingName: nameof(NativeExecutables)));

            if (UniquePatchingIdentifierScope.Length == 0)
                throw new ArgumentException(CreateNotSpecifiedSettingMessage(settingScope: null, settingName: nameof(UniquePatchingIdentifierScope)));

            if (Logging.MainApplicationFileName.Length == 0)
                throw new ArgumentException(CreateNotSpecifiedSettingMessage(settingScope: nameof(Logging), settingName: nameof(Logging.MainApplicationFileName)));
            if (Logging.BootstrapManagedFileName.Length == 0)
                throw new ArgumentException(CreateNotSpecifiedSettingMessage(settingScope: nameof(Logging), settingName: nameof(Logging.BootstrapManagedFileName)));
            if (Logging.BootstrapNativeFileName.Length == 0)
                throw new ArgumentException(CreateNotSpecifiedSettingMessage(settingScope: nameof(Logging), settingName: nameof(Logging.BootstrapNativeFileName)));
        }

        private static string CreateEmptyArrayMessage(string? settingScope, string settingName)
        {
            return $"Configuration \"appsettings.yml\" is malformed. Setting array \"{settingScope}{(settingScope == null ? string.Empty : ".")}{settingName}\" was empty or missing.";
        }

        private static string CreateNotSpecifiedSettingMessage(string? settingScope, string settingName)
        {
            return $"Configuration \"appsettings.yml\" is malformed. Setting \"{settingScope}{(settingScope == null ? string.Empty : ".")}{settingName}\" was not specified.";
        }
    }
}
