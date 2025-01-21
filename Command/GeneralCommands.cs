using KIBAEMON2024_Core.Struct;

namespace KIBAEMON2024_CSharp.Command;

public class GeneralCommands : ICommandGroup
{
    [Command("general", "봇의 응답속도 확인", "ping")]
    public async Task PingAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        await context.Channel.SendMessageAsync("Pong!");
    }

    [Command("general", "입력한 메시지를 따라 말하기", "echo")]
    public async Task EchoAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var text = parameters.Get<string>("text");
        if (string.IsNullOrEmpty(text))
        {
            await context.Channel.SendMessageAsync("반복할 문구를 입력하세요.");
            return;
        }

        await context.Channel.SendMessageAsync(text);
    }
}