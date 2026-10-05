using ElkollegeGuidanceBot.Extensions;
using ElkollegeGuidanceBot.Models;
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

    // TODO: add other buttons

    public static InlineKeyboardButton SelectUserType(UserType userType) =>
        new(userType.GetDescription())
        {
            CallbackData = $"{CallbackStrings.SelectUserType} {(int)userType}"
        };
}
