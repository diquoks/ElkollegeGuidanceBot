using ElkollegeGuidanceBot.Services;
using Telegram.BotAPI.Extensions.LongPolling;

namespace ElkollegeGuidanceBot;

public class Worker(ILogger<Worker> logger, TelegramBot bot) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Worker started.");

        await bot.Client.StartLongPolling(
            bot.OnUpdateAsync,
            bot.OnErrorAsync,
            new LongPollingOptions { DropPendingUpdates = true, Timeout = 10 },
            stoppingToken
        );
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Worker is stopping...");

        return base.StopAsync(cancellationToken);
    }
}
