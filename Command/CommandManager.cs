#nullable enable

using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using KIBAEMON2024_CSharp.Enviroment;

namespace KIBAEMON2024_CSharp.Command;

public class CommandManager
{
    private Bot Bot { get; }
    private CommandService CommandService { get; }

    public CommandManager(Bot bot, CommandService commandService)
    {
        Bot = bot;
        CommandService = commandService;

        commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), null);
        Bot.Client.MessageReceived += HandleCommandAsync;
    }

    ~CommandManager()
    {
        Bot.Client.MessageReceived -= HandleCommandAsync;
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        Bot.Log(message.ToString());

        int argPos = 0;

        if (IsCommandRequest(message, ref argPos))
            return;

        var context = new SocketCommandContext(Bot.Client, message);

        await CommandService.ExecuteAsync(context, argPos, null);
    }

    private bool IsCommandRequest(SocketUserMessage message, ref int argPos)
    {
        return !(message.HasStringPrefix(Bot.Prefix, ref argPos) || message.HasMentionPrefix(Bot.Client.CurrentUser, ref argPos)) || message.Author.IsBot;
    }
}