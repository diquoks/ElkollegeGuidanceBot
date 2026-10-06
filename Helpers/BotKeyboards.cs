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

    public static InlineKeyboardMarkup StartHasActiveTest =>
        new(
            new InlineKeyboardBuilder()
                .Append(BotButtons.StartNewTest)
        );

    public static InlineKeyboardMarkup PersonalDataAgreement =>
        new(
            new InlineKeyboardBuilder()
                .Append(BotButtons.PersonalDataAgreement(true))
                .Append(BotButtons.PersonalDataAgreement(false))
        );

    public static InlineKeyboardMarkup Instructions =>
        new(
            new InlineKeyboardBuilder()
                .Append(BotButtons.InstructionsRead)
        );

    // TODO: add other keyboards

    public static InlineKeyboardMarkup SelectUserType =>
        new(
            new InlineKeyboardBuilder()
                .Append(BotButtons.SelectUserType(UserType.Schoolkid))
                .AppendRow()
                .Append(BotButtons.SelectUserType(UserType.CollegeStudent))
                .Append(BotButtons.SelectUserType(UserType.UniversityStudent))
        );
}
