using Discord;
using KIBAEMON2024_CSharp.Enviroment;

namespace KIBAEMON2024_CSharp;

static class BotRunner
{
    public static async Task Main(string[] args)
    {
        BotManager.Initialize();

        var bot = BotManager.GetBot(args[0]);

        if (bot == null)
        {
            Console.WriteLine("Bot not found.");
            return;
        }

        await bot.Client.LoginAsync(TokenType.Bot, bot.Authorization.Token);
        await bot.Client.StartAsync();
        Console.WriteLine($"Bot {bot.Name} started.");

        await Task.Delay(-1);
    }
}