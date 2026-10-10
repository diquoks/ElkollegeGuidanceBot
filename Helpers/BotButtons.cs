using ElkollegeGuidanceBot.Extensions;
using ElkollegeGuidanceBot.Models.Database;
using ElkollegeGuidanceBot.Models.Guidance.Binary;
using ElkollegeGuidanceBot.Strings;
using Telegram.BotAPI.AvailableTypes;

namespace ElkollegeGuidanceBot.Helpers;

public static class BotButtons
{
    public static InlineKeyboardButton StartTest =>
        new(ButtonStrings.StartTest)
        {
            CallbackData = CallbackStrings.Start
        };

    public static InlineKeyboardButton StartNewTest =>
        new(ButtonStrings.StartNewTest)
        {
            CallbackData = CallbackStrings.Start
        };

    public static InlineKeyboardButton PersonalDataAgreement(bool isAgree) =>
        new(isAgree ? ButtonStrings.Agree : ButtonStrings.Disagree)
        {
            CallbackData = CallbackStrings.PersonalDataAgreementCallback(isAgree)
        };

    public static InlineKeyboardButton InstructionsRead =>
        new(ButtonStrings.Continue)
        {
            CallbackData = CallbackStrings.InstructionsRead
        };

    public static InlineKeyboardButton BlockContinue(int blockIndex) =>
        new(ButtonStrings.Continue)
        {
            CallbackData = CallbackStrings.TestBlockCallback(blockIndex)
        };

    public static InlineKeyboardButton OptionsQuestionAnswer(
        int blockIndex,
        int questionIndex,
        int answerIndex,
        char answerChar
    ) =>
        new(answerChar.ToString())
        {
            CallbackData = CallbackStrings.TestAnswerCallback(blockIndex, questionIndex, answerIndex)
        };

    public static InlineKeyboardButton BinaryQuestionAnswer(
        int blockIndex,
        int questionIndex,
        GuidanceBinaryAnswer answer
    ) =>
        new(answer.GetDescription())
        {
            CallbackData = CallbackStrings.TestAnswerCallback(blockIndex, questionIndex, (int)answer)
        };

    public static InlineKeyboardButton SelectUserType(UserType userType) =>
        new(userType.GetDescription())
        {
            CallbackData = CallbackStrings.SelectUserTypeCallback((int)userType)
        };
}
