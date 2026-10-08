namespace ElkollegeGuidanceBot.Strings;

public static class CallbackStrings
{
    public const string Start =
        "start";

    public const string PersonalDataAgreement =
        "personal_data_agreement";

    public const string InstructionsRead =
        "instructions_read";

    public static string PersonalDataAgreementCallback(bool isAgree) =>
        $"{PersonalDataAgreement} {Convert.ToString(isAgree)}";

    public const string TestBlock =
        "test_block";

    public static string TestBlockCallback(int blockIndex) =>
        $"{TestBlock} {blockIndex}";

    public const string TestAnswer =
        "test_answer";

    public static string TestAnswerCallback(int blockIndex, int questionIndex) =>
        $"{TestAnswer} {blockIndex} {questionIndex}";

    public static string TestAnswerCallback(int blockIndex, int questionIndex, int answer) =>
        $"{TestAnswer} {blockIndex} {questionIndex} {answer}";

    public static string TestAnswerCallback(int blockIndex, int questionIndex, bool answer) =>
        $"{TestAnswer} {blockIndex} {questionIndex} {Convert.ToString(answer)}";

    public const string SelectUserType =
        "select_user_type";

    public static string SelectUserTypeCallback(int userType) =>
        $"{SelectUserType} {userType}";
}
