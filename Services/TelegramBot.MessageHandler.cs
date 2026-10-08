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
                throw new NotImplementedException();
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
}
