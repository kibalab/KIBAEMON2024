#nullable enable

using Discord;
using KIBAEMON2024_CSharp.Enviroment;

namespace KIBAEMON2024_CSharp
{
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

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
    }
}