using ElkollegeGuidanceBot.Services;

namespace ElkollegeGuidanceBot;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services
            // TODO
            // .AddSingleton<GuidanceTestProvider>()
            .AddSingleton<DatabaseManager>()
            .AddSingleton<TelegramBot>()
            .AddHostedService<Worker>();

        var host = builder.Build();
        await host.RunAsync();
    }
}
