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
            updateHandler: bot.OnUpdateAsync,
            errorHandler: bot.OnErrorAsync,
            options: new LongPollingOptions { DropPendingUpdates = true, Timeout = 10 },
            cancellationToken: stoppingToken
        );
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Worker is stopping...");

        return base.StopAsync(cancellationToken);
    }
}
