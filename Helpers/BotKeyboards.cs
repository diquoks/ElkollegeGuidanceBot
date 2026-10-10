using ElkollegeGuidanceBot.Models.Database;
using ElkollegeGuidanceBot.Models.Guidance.Binary;
using ElkollegeGuidanceBot.Models.Guidance.Options;
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

    public static InlineKeyboardMarkup TestBlock(int blockIndex) =>
        new(
            new InlineKeyboardBuilder()
                .Append(BotButtons.BlockContinue(blockIndex))
        );

    public static InlineKeyboardMarkup TestOptionsQuestion(
        int[] answerIndexes,
        int blockIndex,
        int questionIndex
    )
    {
        var keyboardBuilder = new InlineKeyboardBuilder();

        var buttons = answerIndexes.Select((answerIndex, i) =>
            BotButtons.OptionsQuestionAnswer(
                blockIndex,
                questionIndex,
                answerIndex,
                GuidanceOptionsAnswer.GetChar(i)
            )
        );

        foreach (var button in buttons)
            keyboardBuilder.Append(button);

        return new InlineKeyboardMarkup(keyboardBuilder);
    }

    public static InlineKeyboardMarkup TestBinaryQuestion(int blockIndex, int questionIndex) =>
        new(
            new InlineKeyboardBuilder()
                .Append(BotButtons.BinaryQuestionAnswer(blockIndex, questionIndex, GuidanceBinaryAnswer.Yes))
                .Append(BotButtons.BinaryQuestionAnswer(blockIndex, questionIndex, GuidanceBinaryAnswer.ProbablyYes))
                .Append(BotButtons.BinaryQuestionAnswer(blockIndex, questionIndex, GuidanceBinaryAnswer.No))
        );

    public static InlineKeyboardMarkup SelectUserType =>
        new(
            new InlineKeyboardBuilder()
                .Append(BotButtons.SelectUserType(UserType.Schoolkid))
                .AppendRow()
                .Append(BotButtons.SelectUserType(UserType.CollegeStudent))
                .Append(BotButtons.SelectUserType(UserType.UniversityStudent))
        );
}
