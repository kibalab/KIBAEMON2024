#nullable enable

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KIBAEMON2024_CSharp.Command;
using Newtonsoft.Json;

namespace KIBAEMON2024_CSharp.Enviroment;

[Serializable]
public class Bot
{
    public string Name { get; init; }
    public string Prefix { get; set; } = "?";
    public Authorization Authorization { get; init; } = Authorization.Empty;

    [JsonIgnore] public CommandManager CommandManager { get; init; }
    [JsonIgnore] public DiscordSocketClient Client { get; init; } = new(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All, MessageCacheSize = 1000 });

    [JsonIgnore] private CommandService CommandService { get; init; } = new();
    [JsonIgnore] private LoggingService LoggingService { get; init; }

    public Bot(string name = "UnknownBot")
    {
        Name = name;
        LoggingService = new LoggingService(Client, CommandService);
        CommandManager = new CommandManager(this, CommandService);
    }

    public void Log(string message)
    {
        LoggingService.LogAsync(new LogMessage(LogSeverity.Info, Name, message));
    }
}