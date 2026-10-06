using ElkollegeGuidanceBot.Extensions;
using ElkollegeGuidanceBot.Helpers;
using ElkollegeGuidanceBot.Models;
using ElkollegeGuidanceBot.Strings;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.UpdatingMessages;

namespace ElkollegeGuidanceBot.Services;

public partial class TelegramBot
{
    protected override async Task OnCallbackQueryAsync(
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken = default
    )
    {
        if (
            callbackQuery.Message is null ||
            string.IsNullOrEmpty(callbackQuery.Data)
        )
            return;

        var state = await _databaseManager.GetUserStateAsync(callbackQuery.From.Id, cancellationToken);

        _logger.LogBotInteraction(callbackQuery.From, callbackQuery.Data, new { State = state.Type });

        switch (callbackQuery.Data.Split())
        {
            case
            [
                CallbackStrings.Start
            ]:
                state.Clear();
                state.Type = StateType.PersonalDataAgreement;

                await using (
                    var fileStream = new FileStream(
                        _personalDataAgreementPath,
                        FileMode.Open,
                        FileAccess.Read
                    )
                )
                {
                    const string personalDataAgreementKey = "personal_data_agreement";

                    await Client.EditMessageMediaAsync<Message>(
                        new EditMessageMediaArgs(
                            new InputMediaDocument($"attach://{personalDataAgreementKey}")
                            {
                                Caption = MenuStrings.PersonalDataAgreement
                            }
                        )
                        {
                            ChatId = callbackQuery.Message.Chat.Id,
                            MessageId = callbackQuery.Message.MessageId,
                            Files =
                            {
                                {
                                    personalDataAgreementKey,
                                    new InputFile(fileStream, Path.GetFileName(fileStream.Name))
                                }
                            },
                            ReplyMarkup = BotKeyboards.PersonalDataAgreement
                        },
                        cancellationToken
                    );
                }

                break;

            case
            [
                CallbackStrings.PersonalDataAgreement,
                var isAgreeString
            ] when state.Type is StateType.PersonalDataAgreement:
                var isAgree = Convert.ToBoolean(isAgreeString);

                if (!isAgree)
                {
                    state.Clear();

                    await Client.DeleteMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        callbackQuery.Message.MessageId,
                        cancellationToken
                    );

                    await Client.SendMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        MenuStrings.PersonalDataAgreementDisagree,
                        cancellationToken: cancellationToken
                    );

                    break;
                }

                state.Type = StateType.Instructions;

                await Client.EditMessageReplyMarkupAsync(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken
                );

                await Client.SendMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    MenuStrings.Instructions,
                    parseMode: DefaultParseMode,
                    replyMarkup: BotKeyboards.Instructions,
                    cancellationToken: cancellationToken
                );

                break;

            case
            [
                CallbackStrings.InstructionsRead
            ] when state.Type is StateType.Instructions:
                state.Type = StateType.InputFullName;

                await Client.EditMessageReplyMarkupAsync(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken
                );

                await Client.SendMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    MenuStrings.InputFullName,
                    cancellationToken: cancellationToken
                );

                break;

            default:
                await Client.AnswerCallbackQueryAsync(
                    callbackQuery.Id,
                    MenuStrings.TestUnavailable,
                    true,
                    cancellationToken: cancellationToken
                );

                break;
        }

        await _databaseManager.UpsertStateAsync(state, cancellationToken);
    }
}
