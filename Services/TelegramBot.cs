using ElkollegeGuidanceBot.Helpers;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.Extensions;
using Telegram.BotAPI.GettingUpdates;

namespace ElkollegeGuidanceBot.Services;

public partial class TelegramBot : SimpleUpdateHandlerBase
{
    private const string DefaultParseMode = "HTML";

    public readonly ITelegramBotClient Client;

    private readonly long[] _admins;
    private readonly string _botName;

    private readonly DatabaseManager _databaseManager;
    private readonly ILogger<TelegramBot> _logger;

    public TelegramBot(DatabaseManager databaseManager, ILogger<TelegramBot> logger, IConfiguration configuration)
    {
        _databaseManager = databaseManager;
        _logger = logger;

        const string botTokenKey = "Telegram:BotToken";
        var botToken = configuration.GetValue<string>(botTokenKey);

        if (string.IsNullOrWhiteSpace(botToken))
            throw new InvalidOperationException($"{botTokenKey} cannot be empty!");

        _admins = configuration.GetSection("Telegram:Admins").Get<long[]>() ?? [];

        Client = new TelegramBotClient(botToken);

        var botUser = Client.GetMe();
        _botName = botUser.FirstName;

        if (botUser.Username is not null)
            SetBotUserName(botUser.Username);

        Client.SetMyCommands(BotCommands.AllCommands, new BotCommandScopeAllPrivateChats());
    }

    public async Task OnUpdateAsync(
        ITelegramBotClient _,
        Update update,
        CancellationToken cancellationToken = default
    ) => await OnUpdateAsync(update, cancellationToken);

    public Task OnErrorAsync(
        ITelegramBotClient _,
        Exception exp,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            switch (exp)
            {
                case OperationCanceledException when !cancellationToken.IsCancellationRequested:
                    _logger.LogWarning(exp, "Request timed out.");
                    return Task.CompletedTask;

                case OperationCanceledException:
                    throw exp;

                default:
                    _logger.LogError(exp, "An exception occurred.");
                    return Task.CompletedTask;
            }
        }
        catch (Exception e)
        {
            return Task.FromException(e);
        }
    }

    protected override async Task OnExceptionAsync(Exception exp, CancellationToken cancellationToken = default) =>
        await OnErrorAsync(Client, exp, cancellationToken: cancellationToken);
}
