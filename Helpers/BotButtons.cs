using ElkollegeGuidanceBot.Extensions;
using ElkollegeGuidanceBot.Models.Database;
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
            CallbackData = $"{CallbackStrings.PersonalDataAgreement} {Convert.ToString(isAgree)}"
        };

    public static InlineKeyboardButton InstructionsRead =>
        new(ButtonStrings.Continue)
        {
            CallbackData = CallbackStrings.InstructionsRead
        };

    public static InlineKeyboardButton BlockContinue(int blockIndex) =>
        new(ButtonStrings.Continue)
        {
            CallbackData = $"{CallbackStrings.TestBlock} {blockIndex}"
        };

    public static InlineKeyboardButton OptionsQuestionAnswer(int blockIndex, int questionIndex, int answerIndex) =>
        new($"{questionIndex + 1}")
        {
            CallbackData = $"{CallbackStrings.TestAnswer} {blockIndex} {questionIndex} {answerIndex}"
        };

    public static InlineKeyboardButton BinaryQuestionAnswer(int blockIndex, int questionIndex, bool value) =>
        new(value ? ButtonStrings.Yes : ButtonStrings.No)
        {
            CallbackData = $"{CallbackStrings.TestAnswer} {blockIndex} {questionIndex} {Convert.ToString(value)}"
        };

    public static InlineKeyboardButton SelectUserType(UserType userType) =>
        new(userType.GetDescription())
        {
            CallbackData = $"{CallbackStrings.SelectUserType} {(int)userType}"
        };
}
