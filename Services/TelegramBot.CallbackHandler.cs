using System.Diagnostics;
using ElkollegeGuidanceBot.Extensions;
using ElkollegeGuidanceBot.Helpers;
using ElkollegeGuidanceBot.Models.Database;
using ElkollegeGuidanceBot.Models.Guidance;
using ElkollegeGuidanceBot.Models.Guidance.Binary;
using ElkollegeGuidanceBot.Models.Guidance.Options;
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
        var isExpectedCallback = CheckIsExpectedCallback(callbackQuery, state);

        _logger.LogBotInteraction(
            callbackQuery.From,
            callbackQuery.Data,
            new
            {
                State = state.Type,
                IsExpectedCallback = isExpectedCallback
            }
        );

        switch (callbackQuery.Data.Split())
        {
            case
            [
                CallbackStrings.Start
            ]:
            {
                state.Clear();
                state.Type = StateType.PersonalDataAgreement;

                await using var fileStream = new FileStream(
                    _personalDataAgreementPath,
                    FileMode.Open,
                    FileAccess.Read
                );

                const string personalDataAgreementKey = "personal_data_agreement";

                await Client.EditMessageMediaAsync<Message>(
                    args: new EditMessageMediaArgs(
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
                    cancellationToken: cancellationToken
                );

                break;
            }
            case
            [
                CallbackStrings.PersonalDataAgreement,
                var isAgreeString
            ] when state.Type is StateType.PersonalDataAgreement:
            {
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
                    text: _guidanceProvider.Test.Instruction,
                    parseMode: DefaultParseMode,
                    replyMarkup: BotKeyboards.Instructions,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case
            [
                CallbackStrings.InstructionsRead
            ] when state.Type is StateType.Instructions:
            {
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
            }
            case
            [
                CallbackStrings.TestBlock,
                var blockIndexString
            ] when state.Type is StateType.GuidanceTest && isExpectedCallback:
            {
                var blockIndex = int.Parse(blockIndexString);

                const int firstQuestionIndex = 0;

                state = await ProceedToTestQuestionAsync(
                    callbackQuery,
                    state,
                    blockIndex,
                    firstQuestionIndex,
                    cancellationToken
                );

                break;
            }
            case
            [
                CallbackStrings.TestAnswer,
                var blockIndexString,
                var questionIndexString,
                var answerString
            ] when state.Type is StateType.GuidanceTest && isExpectedCallback:
            {
                var blockIndex = int.Parse(blockIndexString);
                var questionIndex = int.Parse(questionIndexString);

                state = SaveAnswerToState(
                    state,
                    blockIndex,
                    questionIndex,
                    answerString
                );

                var block = _guidanceProvider.Test.Blocks[blockIndex];
                questionIndex++;

                if (questionIndex < block.Questions.Count)
                {
                    state = await ProceedToTestQuestionAsync(
                        callbackQuery,
                        state,
                        blockIndex,
                        questionIndex,
                        cancellationToken
                    );

                    break;
                }

                blockIndex++;

                if (blockIndex < _guidanceProvider.Test.Blocks.Length)
                {
                    state = await ProceedToTestBlockAsync(
                        callbackQuery,
                        state,
                        blockIndex,
                        cancellationToken
                    );

                    break;
                }

                state.Type = StateType.SelectUserStatus;
                state.Data.NextExpectedCallback = null;

                await Client.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: MenuStrings.SelectUserStatus,
                    replyMarkup: BotKeyboards.SelectUserStatus,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case
            [
                CallbackStrings.SelectUserStatus,
                var userStatusString
            ] when state.Type is StateType.SelectUserStatus:
            {
                var userStatus = (UserStatus)int.Parse(userStatusString);

                state.Data.UserStatus = userStatus;

                if (userStatus is UserStatus.Other)
                {
                    state.Type = StateType.InputOtherUserStatus;

                    await Client.EditMessageTextAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        text: MenuStrings.InputOtherUserStatus,
                        cancellationToken: cancellationToken
                    );

                    break;
                }

                state.Type = StateType.InputInstitution;

                await Client.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: MenuStrings.InputInstitution,
                    cancellationToken: cancellationToken
                );

                break;
            }
            default:
            {
                var isStateEmpty = state.Type is StateType.Empty;

                await Client.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    text: isStateEmpty
                        ? OtherStrings.Alerts.ButtonUnavailable
                        : OtherStrings.Alerts.HasActiveTest,
                    showAlert: true,
                    cancellationToken: cancellationToken
                );

                break;
            }
        }

        await _databaseManager.UpsertStateAsync(state, cancellationToken);
    }

    private bool CheckIsExpectedCallback(CallbackQuery callbackQuery, State state)
    {
        var expected = state.Data.NextExpectedCallback;
        var actual = callbackQuery.Data;

        if (string.IsNullOrEmpty(actual))
            throw new UnreachableException();

        return string.IsNullOrEmpty(expected) ||
               actual == expected ||
               actual.StartsWith($"{expected} ");
    }

    private async Task<State> ProceedToTestBlockAsync(
        CallbackQuery callbackQuery,
        State state,
        int blockIndex,
        CancellationToken cancellationToken = default
    )
    {
        if (callbackQuery.Message is null)
            throw new UnreachableException();

        var block = _guidanceProvider.Test.Blocks[blockIndex];

        state.Data.NextExpectedCallback = CallbackStrings.TestBlockCallback(blockIndex);

        await Client.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: MenuStrings.TestBlock(block),
            parseMode: DefaultParseMode,
            replyMarkup: BotKeyboards.TestBlock(blockIndex),
            cancellationToken: cancellationToken
        );

        return state;
    }

    private async Task<State> ProceedToTestQuestionAsync(
        CallbackQuery callbackQuery,
        State state,
        int blockIndex,
        int questionIndex,
        CancellationToken cancellationToken = default
    )
    {
        if (callbackQuery.Message is null)
            throw new UnreachableException();

        var block = _guidanceProvider.Test.Blocks[blockIndex];

        string messageText;
        ReplyMarkup replyMarkup;

        switch (block.Type)
        {
            case GuidanceBlockType.Options:
            {
                var optionsBlock = (GuidanceOptionsBlock)block;
                var question = optionsBlock.Questions[questionIndex];

                var shuffledAnswersWithIndexes = question.Answers
                    .Select((answer, i) => (Index: i, Answer: answer))
                    .Shuffle()
                    .ToArray();

                messageText = MenuStrings.TestOptionsQuestion(
                    question with { Answers = [.. shuffledAnswersWithIndexes.Select(tuple => tuple.Answer)] }
                );
                replyMarkup = BotKeyboards.TestOptionsQuestion(
                    [.. shuffledAnswersWithIndexes.Select(tuple => tuple.Index)],
                    blockIndex,
                    questionIndex
                );

                break;
            }
            case GuidanceBlockType.Binary:
            {
                var binaryBlock = (GuidanceBinaryBlock)block;
                var question = binaryBlock.Questions[questionIndex];

                messageText = MenuStrings.TestBinaryQuestion(question);
                replyMarkup = BotKeyboards.TestBinaryQuestion(blockIndex, questionIndex);

                break;
            }
            default:
                throw new UnreachableException();
        }

        state.Data.NextExpectedCallback = CallbackStrings.TestAnswerCallback(blockIndex, questionIndex);

        await Client.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: messageText,
            parseMode: DefaultParseMode,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken
        );

        return state;
    }

    private State SaveAnswerToState(
        State state,
        int blockIndex,
        int questionIndex,
        string answerString
    )
    {
        var block = _guidanceProvider.Test.Blocks[blockIndex];

        switch (block.Type)
        {
            case GuidanceBlockType.Options:
            {
                var optionsBlock = (GuidanceOptionsBlock)block;
                var question = optionsBlock.Questions[questionIndex];
                var answer = question.Answers[int.Parse(answerString)];

                var typeRating = state.Data.TypeRatings.GetValueOrDefault(answer.TypeIndex);
                state.Data.TypeRatings[answer.TypeIndex] = typeRating + 1;

                break;
            }
            case GuidanceBlockType.Binary:
            {
                var binaryBlock = (GuidanceBinaryBlock)block;
                var question = binaryBlock.Questions[questionIndex];
                var answerValue = int.Parse(answerString);

                var typeRating = state.Data.TypeRatings.GetValueOrDefault(question.TypeIndex);
                state.Data.TypeRatings[question.TypeIndex] = typeRating + answerValue;

                break;
            }
            default:
                throw new UnreachableException();
        }

        return state;
    }
}
