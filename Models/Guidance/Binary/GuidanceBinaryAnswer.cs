using System.ComponentModel;
using ElkollegeGuidanceBot.Strings;

namespace ElkollegeGuidanceBot.Models.Guidance.Binary;

public enum GuidanceBinaryAnswer
{
    [Description(OtherStrings.GuidanceBinaryAnswer.Yes)]
    Yes = 2,

    [Description(OtherStrings.GuidanceBinaryAnswer.LikelyYes)]
    LikelyYes = 1,

    [Description(OtherStrings.GuidanceBinaryAnswer.LikelyNo)]
    LikelyNo = 0,

    [Description(OtherStrings.GuidanceBinaryAnswer.No)]
    No = -1
}
