using ElkollegeGuidanceBot.Extensions;
using ElkollegeGuidanceBot.Models.Database;
using ElkollegeGuidanceBot.Models.Guidance;
using ElkollegeGuidanceBot.Models.Guidance.Binary;
using ElkollegeGuidanceBot.Models.Guidance.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ElkollegeGuidanceBot.Services;

public class GuidanceProvider
{
    public readonly GuidanceTest Test;

    public GuidanceProvider(IConfiguration configuration)
    {
        const string guidanceTestKey = "Assets:GuidanceTestPath";
        var guidanceTestPath = configuration.GetValue<string>(guidanceTestKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(guidanceTestPath, guidanceTestKey);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddKeyValueTypeDiscriminator<IGuidanceBlock>(
                        nameof(IGuidanceBlock.Type).ToLower(),
                        new Dictionary<string, Type>
                        {
                            { nameof(GuidanceBlockType.Options), typeof(GuidanceOptionsBlock) },
                            { nameof(GuidanceBlockType.Binary), typeof(GuidanceBinaryBlock) }
                        }
                    );
                }
            )
            .Build();

        Test = deserializer.Deserialize<GuidanceTest>(File.ReadAllText(guidanceTestPath));
    }

    public GuidanceType GetPossibleType(State state)
    {
        var maxRating = state.Data.TypeRatings.Values.Max();
        var possibleTypeIndex = state.Data.TypeRatings
            .Where(pair => pair.Value == maxRating)
            .Select(pair => pair.Key)
            .Shuffle()
            .First();

        return Test.Types[possibleTypeIndex];
    }

    public Result GetResult(State state, GuidanceType possibleType)
    {
        var ratingsSum = state.Data.TypeRatings.Values.Sum();
        var ratingRatios = state.Data.TypeRatings
            .OrderByDescending(pair => pair.Value)
            .Select(pair => $"{Test.Types[pair.Key].Class}: {(double)pair.Value / ratingsSum:P}")
            .ToArray();

        return new Result
        {
            UserId = state.UserId,
            FullName = state.Data.FullName,
            PhoneNumber = state.Data.PhoneNumber,
            UserStatus = state.Data.UserStatus is UserStatus.Other
                ? state.Data.OtherUserStatus
                : state.Data.UserStatus.GetDescription(),
            Institution = state.Data.Institution,
            CurrentCourse = state.Data.CurrentCourse,
            PossibleType = possibleType.Class,
            TypeRatings = string.Join(" | ", ratingRatios),
            CreatedTimestamp = DateTimeOffset.Now
        };
    }
}
