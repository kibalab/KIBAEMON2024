using System.Reflection;

namespace KIBAEMON2024_CSharp.System;

public class CommandInfo(MethodInfo method, CommandAttribute attribute, object instance)
{
    public MethodInfo Method { get; } = method;
    public CommandAttribute Attribute { get; } = attribute;
    public object DeclaringInstance { get; } = instance;

    public async Task ExecuteAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var result = Method.Invoke(DeclaringInstance, [bot, context, parameters]);
        if (result is Task task)
        {
            await task;
        }
    }
}