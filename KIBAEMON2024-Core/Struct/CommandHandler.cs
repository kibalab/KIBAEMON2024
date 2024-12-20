using Discord.Commands;
using Discord.WebSocket;

namespace KIBAEMON2024_CSharp.System;

public class CommandHandler(CommandManager commandManager, Bot bot)
{
    private CommandManager CommandManager { get; } = commandManager;
    private Bot Bot { get; } = bot;

    public async Task OnMessageReceived(SocketMessage rawMessage)
    {
        if (rawMessage.Author.IsBot) return;

        if (rawMessage is SocketUserMessage message)
        {
            var argPos = 0;

            if (message.HasStringPrefix(Bot.Prefix, ref argPos))
            {
                var context = new CommandContext(message);

                var content = message.Content[argPos..].Trim();
                var parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0) return;

                var alias = parts[0];
                var args = parts.Skip(1).ToArray();

                var parametersDict = new Dictionary<string, object>
                {
                    ["text"] = string.Join(' ', args)
                };

                var commandParams = new CommandParameters(parametersDict);

                var command = CommandManager.GetCommand(alias);
                if (command != null) command.ExecuteAsync(Bot, context, commandParams);
            }
        }
    }
}