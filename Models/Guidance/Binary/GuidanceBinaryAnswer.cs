using System.ComponentModel;
using ElkollegeGuidanceBot.Strings;

namespace ElkollegeGuidanceBot.Models.Guidance.Binary;

public enum GuidanceBinaryAnswer
{
    [Description(OtherStrings.GuidanceBinaryAnswer.Yes)]
    Yes = 2,

    [Description(OtherStrings.GuidanceBinaryAnswer.ProbablyYes)]
    ProbablyYes = 1,

    [Description(OtherStrings.GuidanceBinaryAnswer.No)]
    No = 0
}
