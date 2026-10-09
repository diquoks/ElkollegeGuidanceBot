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
        var maxRating = state.Data.GuidanceTypeRatings.Values.Max();

        var possibleTypeIndex = state.Data.GuidanceTypeRatings
            .Where(pair => pair.Value == maxRating)
            .Select(pair => pair.Key)
            .Shuffle()
            .First();

        return Test.Types[possibleTypeIndex];
    }
}
