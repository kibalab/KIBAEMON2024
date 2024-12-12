using Discord;

namespace KIBAEMON2024_CSharp.System;

public class CommandContext(IMessage message)
{
    public IMessage Message { get; } = message;
    public IUser User { get; } = message.Author;
    public IMessageChannel Channel { get; } = message.Channel;
    public IGuild? Guild { get; } = (message.Channel as IGuildChannel)?.Guild;
}