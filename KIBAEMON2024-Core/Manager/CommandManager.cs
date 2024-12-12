using System.Reflection;
using KIBAEMON2024_Core;

namespace KIBAEMON2024_CSharp.System;

public class CommandManager
{
    public CommandHandler CommandHandler { get; }

    private readonly Dictionary<string, CommandInfo> _commands = new();

    public CommandManager(Bot bot)
    {
        CommandHandler = new CommandHandler(this, bot);

        bot.Client.MessageReceived += CommandHandler.OnMessageReceived;
    }

    public void RegisterCommandsFromAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (!typeof(ICommandGroup).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract)
            {
                continue;
            }

            Console.WriteLine($"타입 검사: {type.Name}");

            var instance = (ICommandGroup)Activator.CreateInstance(type)!;
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var cmdAttr = method.GetCustomAttribute<CommandAttribute>();
                if (cmdAttr != null)
                {
                    // 메소드 파라미터 체크 (Bot, CommandContext, CommandParameters) 등 검증
                    var parameters = method.GetParameters();
                    if (parameters.Length == 3 &&
                        parameters[0].ParameterType == typeof(Bot) &&
                        parameters[1].ParameterType == typeof(CommandContext) &&
                        parameters[2].ParameterType == typeof(CommandParameters))
                    {
                        var commandInfo = new CommandInfo(method, cmdAttr, instance);
                        _commands[cmdAttr.Alias.First().ToLowerInvariant()] = commandInfo;
                        Console.WriteLine($"명령어 등록: {cmdAttr.Alias.First()}");
                    }
                    else
                    {
                        // 필수 시그니처를 만족하지 않는 경우 예외 처리
                        throw new InvalidOperationException($"'{method.Name}' 메소드는 Bot, CommandContext, CommandParameters 파라미터를 가져야 합니다.");
                    }
                }
            }
        }
    }

    public CommandInfo? GetCommand(string alias)
    {
        _commands.TryGetValue(alias.ToLowerInvariant(), out var commandInfo);
        return commandInfo;
    }

    public IEnumerable<CommandInfo> GetAllCommands() => _commands.Values;
}