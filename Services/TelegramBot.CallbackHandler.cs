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
                        args: new EditMessageMediaArgs(
                            new InputMediaDocument($"attach://{personalDataAgreementKey}")
                            {
                                Caption = MenuStrings.PersonalDataAgreement
                            }
                        )
                        {
                            BusinessConnectionId = null,
                            ChatId = callbackQuery.Message.Chat.Id,
                            MessageId = callbackQuery.Message.MessageId,
                            Files =
                            {
                                {
                                    personalDataAgreementKey,
                                    new InputFile(fileStream, Path.GetFileName(fileStream.Name))
                                }
                            },
                            ReplyMarkup = BotKeyboards.PersonalDataAgreement,
                        },
                        cancellationToken: cancellationToken
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
                        chatId: callbackQuery.Message.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        cancellationToken: cancellationToken
                    );

                    await Client.SendMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: MenuStrings.PersonalDataAgreementDisagree,
                        cancellationToken: cancellationToken
                    );

                    break;
                }

                state.Type = StateType.Instructions;

                await Client.EditMessageReplyMarkupAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken
                );

                await Client.SendMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: MenuStrings.Instructions,
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
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken
                );

                await Client.SendMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: MenuStrings.InputFullName,
                    cancellationToken: cancellationToken
                );

                break;

            default:
                await Client.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    text: MenuStrings.TestUnavailable,
                    showAlert: true,
                    cancellationToken: cancellationToken
                );

                break;
        }

        await _databaseManager.UpsertStateAsync(state, cancellationToken);
    }
}
