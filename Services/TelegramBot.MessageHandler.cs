using ElkollegeGuidanceBot.Extensions;
using ElkollegeGuidanceBot.Helpers;
using ElkollegeGuidanceBot.Models.Database;
using ElkollegeGuidanceBot.Strings;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.Extensions.Commands;

namespace ElkollegeGuidanceBot.Services;

public partial class TelegramBot
{
    protected override async Task OnMessageAsync(
        Message message,
        CancellationToken cancellationToken = default
    )
    {
        if (
            message.From is null ||
            message.From.Id == TelegramConstants.TelegramId ||
            string.IsNullOrWhiteSpace(message.Text)
        )
            return;

        if (BotCommandParser.TryParse(message, out _))
        {
            await base.OnMessageAsync(message, cancellationToken);
            return;
        }

        var state = await _databaseManager.GetUserStateAsync(message.From.Id, cancellationToken);

        _logger.LogBotInteraction(message.From, $"\"{message.Text}\"", new { State = state.Type });

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (state.Type)
        {
            case StateType.InputFullName:
            {
                state.Data.FullName = message.Text;
                state.Type = StateType.InputPhoneNumber;

                await Client.SendMessageAsync(
                    chatId: message.Chat.Id,
                    text: MenuStrings.InputPhoneNumber,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case StateType.InputPhoneNumber:
            {
                if (
                    message.Entities?.FirstOrDefault(entity => entity.Type == MessageEntityTypes.PhoneNumber) is
                    { } phoneNumberEntity
                )
                {
                    state.Data.PhoneNumber = message.Text.Substring(phoneNumberEntity.Offset, phoneNumberEntity.Length);
                }
                else if (state.Data.PhoneNumberRetries < 3)
                {
                    await Client.SendMessageAsync(
                        chatId: message.Chat.Id,
                        text: MenuStrings.InputPhoneNumberError,
                        parseMode: DefaultParseMode,
                        cancellationToken: cancellationToken
                    );

                    state.Data.PhoneNumberRetries++;

                    break;
                }

                state.Type = StateType.GuidanceTest;

                const int firstBlockIndex = 0;

                state = await ProceedToTestBlockAsync(
                    message,
                    state,
                    firstBlockIndex,
                    cancellationToken
                );

                break;
            }
            case StateType.InputInstitution:
            {
                state.Data.Institution = message.Text;

                if (state.Data.UserType is UserType.Schoolkid)
                {
                    state = await SendTestResultsAsync(
                        message,
                        state,
                        cancellationToken
                    );

                    break;
                }

                state.Type = StateType.InputCurrentCourse;

                await Client.SendMessageAsync(
                    chatId: message.Chat.Id,
                    text: MenuStrings.InputCurrentCourse,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case StateType.InputCurrentCourse:
            {
                state.Data.CurrentCourse = message.Text;

                state = await SendTestResultsAsync(
                    message,
                    state,
                    cancellationToken
                );

                break;
            }
        }

        await _databaseManager.UpsertStateAsync(state, cancellationToken);

        await base.OnMessageAsync(message, cancellationToken);
    }

    private async Task<State> ProceedToTestBlockAsync(
        Message message,
        State state,
        int blockIndex,
        CancellationToken cancellationToken = default
    )
    {
        var block = _guidanceProvider.Test.Blocks[blockIndex];

        state.Data.NextExpectedCallback = CallbackStrings.TestBlockCallback(blockIndex);

        await Client.SendMessageAsync(
            chatId: message.Chat.Id,
            text: MenuStrings.TestBlock(block),
            parseMode: DefaultParseMode,
            replyMarkup: BotKeyboards.TestBlock(blockIndex),
            cancellationToken: cancellationToken
        );

        return state;
    }

    private async Task<State> SendTestResultsAsync(
        Message message,
        State state,
        CancellationToken cancellationToken = default
    )
    {
        var possibleType = _guidanceProvider.GetPossibleType(state);

        await Client.SendMessageAsync(
            chatId: message.Chat.Id,
            text: MenuStrings.TestResults(possibleType),
            parseMode: DefaultParseMode,
            cancellationToken: cancellationToken
        );

        await _databaseManager.InsertResultAsync(
            new Result
            {
                UserId = message.From!.Id,
                FullName = state.Data.FullName ?? string.Empty,
                PhoneNumber = state.Data.PhoneNumber ?? string.Empty,
                UserType = state.Data.UserType?.GetDescription() ?? string.Empty,
                Institution = state.Data.Institution ?? string.Empty,
                CurrentCourse = state.Data.CurrentCourse ?? string.Empty,
                PossibleType = possibleType.Class,
                CreatedTimestamp = DateTimeOffset.Now
            },
            cancellationToken
        );

        state.Clear();

        return state;
    }
}
