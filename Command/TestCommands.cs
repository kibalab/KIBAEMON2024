#nullable enable

using Discord.Commands;

namespace KIBAEMON2024_CSharp.Command;

[Group("test")]
public class TestCommands : ModuleBase<SocketCommandContext>
{
    [Command("message")]
    public async Task Message()
    {
        await Context.Channel.SendMessageAsync("Hello, World!");
    }
}