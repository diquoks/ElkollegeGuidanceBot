using ElkollegeGuidanceBot.Services;
using Telegram.BotAPI.GettingUpdates;

namespace ElkollegeGuidanceBot;

public class Worker(ILogger<Worker> logger, TelegramBot bot) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Worker started.");

        var updates = await GetUpdatesWithReconnectAsync(cancellationToken: stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
            if (updates.Length != 0)
            {
                await Parallel.ForEachAsync(updates, stoppingToken, OnUpdateAsync);

                updates = await GetUpdatesWithReconnectAsync(updates.Last().UpdateId + 1, stoppingToken);
            }
            else
            {
                updates = await GetUpdatesWithReconnectAsync(cancellationToken: stoppingToken);
            }
    }

    private async Task<Update[]> GetUpdatesWithReconnectAsync(
        int? offset = null,
        CancellationToken cancellationToken = default
    )
    {
        while (!cancellationToken.IsCancellationRequested)
            try
            {
                return
                [
                    .. await bot.Client
                        .GetUpdatesAsync(offset, timeout: 10, cancellationToken: cancellationToken)
                        .ConfigureAwait(false)
                ];
            }
            catch (OperationCanceledException e) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(e, "Long polling request timed out, retrying...");
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                logger.LogError(e, "An exception occurred during long polling, retrying in 10 seconds...");

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }

        return [];
    }

    private async ValueTask OnUpdateAsync(Update update, CancellationToken cancellationToken = default) =>
        await bot.OnUpdateAsync(update, cancellationToken);

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Worker is stopping...");

        return base.StopAsync(cancellationToken);
    }
}
