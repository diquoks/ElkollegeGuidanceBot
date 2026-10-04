using ElkollegeGuidanceBot.Models;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.Extensions;

namespace ElkollegeGuidanceBot.Helpers;

public static class BotKeyboards
{
    public static InlineKeyboardMarkup Start =>
        new(
            new InlineKeyboardBuilder()
                .Append(BotButtons.StartTest)
        );

    public static InlineKeyboardMarkup StartHasActiveTest(string lastCallback) =>
        new(
            new InlineKeyboardBuilder()
                .Append(BotButtons.StartNewTest)
                .Append(BotButtons.ResumePreviousTest(lastCallback))
        );

    public static InlineKeyboardMarkup PersonalDataAgreement =>
        new(
            new InlineKeyboardBuilder()
                .Append(BotButtons.PersonalDataAgreementAgree)
                .Append(BotButtons.PersonalDataAgreementDisagree)
        );

    // TODO: add other keyboards

    public static InlineKeyboardMarkup SelectUserType =>
        new(
            new InlineKeyboardBuilder()
                .Append(BotButtons.SelectUserType(UserType.Schoolkid))
                .AppendRow()
                .Append(BotButtons.SelectUserType(UserType.CollegeStudent))
                .Append(BotButtons.SelectUserType(UserType.CollegeStudent))
        );
}
