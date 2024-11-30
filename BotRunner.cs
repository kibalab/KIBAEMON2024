using Discord;
using Discord.WebSocket;
using KIBAEMON2024_CSharp.Enviroment;

namespace KIBAEMON2024_CSharp
{
    static class BotRunner
    {
        public static DiscordSocketClient Client { get; set; } = new();

        public static async Task Main(string[] args)
        {
            BotEnvironment.Initialize();

            Client = new DiscordSocketClient();
            Client.Log += Log;
            await Client.LoginAsync(TokenType.Bot, args.Any() ? BotEnvironment.Bots[args[0]].Authorization.Token : BotEnvironment.Bots.First().Value.Authorization.Token);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
    }
}