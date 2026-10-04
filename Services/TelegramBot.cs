using ElkollegeGuidanceBot.Helpers;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.Extensions;

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

    protected override Task OnExceptionAsync(Exception exp, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogError(exp, "An exception occurred while processing updates.");
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            return Task.FromException(e);
        }
    }
}
