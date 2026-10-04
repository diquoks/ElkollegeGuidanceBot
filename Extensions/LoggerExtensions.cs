using Telegram.BotAPI.AvailableTypes;

namespace ElkollegeGuidanceBot.Extensions;

public static class LoggerExtensions
{
    extension(ILogger logger)
    {
        public void LogBotInteraction(User user, string interaction, object? properties = null)
        {
            if (!logger.IsEnabled(LogLevel.Information))
                return;

            var userInfo = user.LastName is not null
                ? $"{user.FirstName} {user.LastName}"
                : user.FirstName;

            if (user.Username is not null)
                userInfo = $"{userInfo} | @{user.Username}";

            logger.LogInformation(
                "{UserInfo} ({UserId}) — {Interaction} | {@Properties}",
                userInfo,
                user.Id,
                interaction,
                properties
            );
        }
    }
}
