using System.ComponentModel;

namespace ElkollegeGuidanceBot.Models;

public enum UserType
{
    [Description("Школьник")]
    Schoolkid,

    [Description("Студент СПО")]
    CollegeStudent,

    [Description("Студент вуза")]
    UniversityStudent
}
