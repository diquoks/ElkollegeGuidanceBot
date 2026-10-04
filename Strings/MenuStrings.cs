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

    // TODO: consider moving to .yaml
    public static string Instruction =>
        """
        Тестирование не является диагностическим инструментом и не даёт окончательного ответа.
        Его цель — запустить размышление и помочь увидеть свои склонности.

        <b>В опросе нет правильных или неправильных ответов, выбирай тот вариант, который больше всего похож на тебя.</b>
        """;

    public static string InputFullName =>
        "Введи своё полное ФИО:";

    public static string InputPhoneNumber =>
        "Введи свой телефонный номер:";

    public static string InputPhoneNumberError =>
        $"""
         <b>Некорректный формат!</b>
         Попробуй ввести телефонный номер в следующем формате:
         <pre>+7(987)654-32-10</pre>

         {InputPhoneNumber}
         """;

    // TODO
    // public static string TestBlock() =>
    //     """
    //     <b>Блок {} | {} ({} вопросов)</b>
    //     {}
    //     """;
    //
    // public static string TestQuestionTitle() =>
    //     "<b>Вопрос {} | {}</b>";
    //
    // public static string TestBinaryQuestion() =>
    //     TestQuestionTitle();
    //
    // public static string TestOptionsAnswer() =>
    //     "{}. {}";
    //
    // public static string TestOptionsQuestion() =>
    //     """
    //     {TestQuestionTitle}
    //
    //     {}
    //     """;

    public static string SelectUserType =>
        "Выбери свой текущий статус:";

    public static string InputInstitution =>
        "Введи название своего текущего учебного заведения:";

    public static string InputCurrentCourse =>
        "Введи направление, на котором сейчас обучаешься:";

    // TODO
    // public static string PossibleType() =>
    //     """
    //     <b>Тестирование окончено!</b>
    //     Твой тип: {} ({}).
    //
    //     Подходящие профессии: {}.
    //     """;
}
