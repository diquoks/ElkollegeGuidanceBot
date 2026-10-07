using ElkollegeGuidanceBot.Models.Guidance;
using ElkollegeGuidanceBot.Models.Guidance.Binary;
using ElkollegeGuidanceBot.Models.Guidance.Options;

namespace ElkollegeGuidanceBot.Strings;

public static class MenuStrings
{
    public static string Start(string botName) =>
        $"""
         <b>Добро пожаловать в {botName}!</b>

         Здесь ты сможешь пройти профориентационное тестирование и узнать, какие профессии подходят тебе больше всего.
         """;

    public static string StartHasActiveTest =>
        "У тебя уже есть активное тестирование, хочешь начать новое?";

    public static string PersonalDataAgreement =>
        "Продолжая, ты соглашаешься с политикой обработки персональных данных.";

    public static string PersonalDataAgreementDisagree =>
        "Спасибо за уделённое время!";

    public static string InputFullName =>
        "Введи своё полное ФИО:";

    public static string InputPhoneNumber =>
        "Введи свой телефонный номер (без пробелов):";

    public static string InputPhoneNumberError =>
        """
        <b>Некорректный формат!</b>
        Попробуй ввести телефонный номер в следующем формате:
        <pre>+7(987)654-32-10</pre>
        """;

    public static string TestBlock(GuidanceBlock block, int blockNumber) =>
        $"""
         <b>Блок {blockNumber} | {block.Title}</b>
         {block.Hint}
         """;

    private static string TestQuestionTitle(GuidanceBlockQuestion question, int questionNumber) =>
        $"<b>Вопрос {questionNumber} | {question.Question}</b>";

    public static string TestOptionsQuestion(GuidanceOptionsQuestion question, int questionNumber) =>
        $"""
         {TestQuestionTitle(question, questionNumber)}

         {string.Join('\n', question.Answers.Select((answer, i) => $"{i + 1}. {answer.Answer}"))}
         """;

    public static string TestBinaryQuestion(GuidanceBinaryQuestion question, int questionNumber) =>
        TestQuestionTitle(question, questionNumber);

    public static string SelectUserType =>
        "Выбери свой текущий статус:";

    public static string InputInstitution =>
        "Введи название своего текущего учебного заведения:";

    public static string InputCurrentCourse =>
        "Введи направление, на котором сейчас обучаешься:";

    public static string PossibleType(GuidanceType type) =>
        $"""
         <b>Тестирование окончено!</b>
         Твой тип: {type.Class} ({type.Name}).

         Подходящие профессии: {string.Join(", ", type.Professions)}.
         """;

    #region Alerts

    public static string TestUnavailable =>
        "Данное тестирование недоступно, начни новое!";

    #endregion
}
