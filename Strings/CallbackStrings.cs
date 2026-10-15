namespace ElkollegeGuidanceBot.Strings;

public static class CallbackStrings
{
    public const string Start =
        "start";

    public const string PersonalDataAgreement =
        "personal_data_agreement";

    public static string PersonalDataAgreementCallback(bool isAgree) =>
        $"{PersonalDataAgreement} {isAgree}";

    public const string InstructionsRead =
        "instructions_read";

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

    public const string SelectUserStatus =
        "select_user_status";

    public static string SelectUserStatusCallback(int userStatus) =>
        $"{SelectUserStatus} {userStatus}";
}
