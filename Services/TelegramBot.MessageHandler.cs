using ElkollegeGuidanceBot.Extensions;
using ElkollegeGuidanceBot.Models;
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
                state.Type = StateType.InputPhoneNumber;
                state.Data.FullName = message.Text;

                await Client.SendMessageAsync(
                    chatId: message.Chat.Id,
                    text: MenuStrings.InputPhoneNumber,
                    cancellationToken: cancellationToken
                );

                break;
            case StateType.InputPhoneNumber:
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

                // TODO: send first block

                break;
        }

        await _databaseManager.UpsertStateAsync(state, cancellationToken);

        await base.OnMessageAsync(message, cancellationToken);
    }
}
