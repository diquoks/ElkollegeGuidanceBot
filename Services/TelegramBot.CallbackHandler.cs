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

        _logger.LogBotInteraction(callbackQuery.From, callbackQuery.Data, new { State = state.Type });

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
            ] when state.Type is StateType.GuidanceTest && callbackQuery.Data.Contains(state.Data.NextExpectedCallback):
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
            ] when state.Type is StateType.GuidanceTest && callbackQuery.Data.Contains(state.Data.NextExpectedCallback):
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

                state.Type = StateType.SelectUserType;

                await Client.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: MenuStrings.SelectUserType,
                    replyMarkup: BotKeyboards.SelectUserType,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case
            [
                CallbackStrings.SelectUserType,
                var userTypeString
            ] when state.Type is StateType.SelectUserType:
            {
                var userType = (UserType)int.Parse(userTypeString);

                state.Data.UserType = userType;
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
                await Client.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    text: MenuStrings.TestUnavailable,
                    showAlert: true,
                    cancellationToken: cancellationToken
                );

                break;
            }
        }

        await _databaseManager.UpsertStateAsync(state, cancellationToken);
    }

    private async Task<State> ProceedToTestBlockAsync(
        CallbackQuery callbackQuery,
        State state,
        int blockIndex,
        CancellationToken cancellationToken = default
    )
    {
        var block = _guidanceProvider.Test.Blocks[blockIndex];

        state.Data.NextExpectedCallback = CallbackStrings.TestBlockCallback(blockIndex);

        await Client.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message!.MessageId,
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
                throw new ArgumentOutOfRangeException(nameof(block.Type), block.Type, "Unknown block type.");
        }

        state.Data.NextExpectedCallback = CallbackStrings.TestAnswerCallback(blockIndex, questionIndex);

        await Client.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message!.MessageId,
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
                var typeIndex = int.Parse(answerString);

                var typeRating = state.Data.GuidanceTypeRatings.GetValueOrDefault(typeIndex);
                state.Data.GuidanceTypeRatings[typeIndex] = ++typeRating;

                break;
            }
            case GuidanceBlockType.Binary:
            {
                var binaryBlock = (GuidanceBinaryBlock)block;
                var question = binaryBlock.Questions[questionIndex];
                var answer = Convert.ToBoolean(answerString);

                if (answer)
                {
                    var typeRating = state.Data.GuidanceTypeRatings.GetValueOrDefault(question.TypeIndex);
                    state.Data.GuidanceTypeRatings[question.TypeIndex] = ++typeRating;
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(block.Type), block.Type, "Unknown block type.");
        }

        return state;
    }
}
