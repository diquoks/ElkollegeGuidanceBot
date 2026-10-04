using System.ComponentModel;
using System.Reflection;

namespace ElkollegeGuidanceBot.Extensions;

public static class EnumExtensions
{
    extension(Enum @enum)
    {
        public string GetDescription()
        {
            var attribute = @enum
                .GetType()
                .GetField(@enum.ToString())
                ?.GetCustomAttribute<DescriptionAttribute>();

            return attribute?.Description ?? @enum.ToString();
        }
    }
}
