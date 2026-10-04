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

    public static InlineKeyboardButton ResumePreviousTest(string lastCallback) =>
        new(ButtonStrings.ResumePreviousTest)
        {
            CallbackData = lastCallback
        };


    public static InlineKeyboardButton StartNewTest =>
        new(ButtonStrings.StartNewTest)
        {
            CallbackData = CallbackStrings.Start
        };

    public static InlineKeyboardButton PersonalDataAgreementAgree =>
        new(ButtonStrings.Agree)
        {
            CallbackData = $"{CallbackStrings.PersonalDataAgreement} 1"
        };

    public static InlineKeyboardButton PersonalDataAgreementDisagree =>
        new(ButtonStrings.Disagree)
        {
            CallbackData = $"{CallbackStrings.PersonalDataAgreement} 0"
        };

    // TODO: add other buttons

    public static InlineKeyboardButton SelectUserType(UserType userType) =>
        new(userType.GetDescription())
        {
            CallbackData = $"{CallbackStrings.SelectUserType} {(int)userType}"
        };
}
