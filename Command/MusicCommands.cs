using Discord;
using Discord.WebSocket;
using KIBAEMON2024_Audio;
using KIBAEMON2024_Core;
using KIBAEMON2024_Core.Struct;
using KIBAEMON2024_CSharp.System;

namespace KIBAEMON2024_CSharp.Command;

public class MusicCommands : ICommandGroup
{
    [Command("music", "YouTube URL을 재생", "play")]
    public async Task PlayAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        try
        {
            var url = parameters.Get<string>("text");
            var player = bot.GetService<AudioPlayerService>();

            if (string.IsNullOrEmpty(url))
            {
                await context.Channel.SendMessageAsync("재생할 유튜브 URL을 입력해주세요.");
                return;
            }

            // 길드 ID 획득
            var guildId = (context.Guild as SocketGuild)?.Id;
            if (guildId == null)
            {
                await context.Channel.SendMessageAsync("길드에서만 사용할 수 있는 명령입니다.");
                return;
            }

            var voiceChannel = (context.User as IVoiceState)?.VoiceChannel ?? throw new Exception("음성 채널에 접속해주세요.");

            await player.EnqueueAsync(guildId.Value, voiceChannel.Id, url, (ISocketMessageChannel)context.Channel);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Command("music", "재생 중지", "stop")]
    public async Task StopAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var player = bot.GetService<AudioPlayerService>();
        var guildId = (context.Guild as SocketGuild)?.Id;

        if (guildId == null)
        {
            await context.Channel.SendMessageAsync("길드에서만 사용할 수 있습니다.");
            return;
        }

        await player.StopAsync(guildId.Value);
        await context.Channel.SendMessageAsync("재생을 중지했습니다.");
    }

    [Command("music", "다음 곡 재생", "skip")]
    public async Task SkipAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var player = bot.GetService<AudioPlayerService>();
        var guildId = (context.Guild as SocketGuild)?.Id;

        if (guildId == null)
        {
            await context.Channel.SendMessageAsync("길드에서만 사용할 수 있습니다.");
            return;
        }

        await player.SkipAsync(guildId.Value);
        await context.Channel.SendMessageAsync("다음 곡을 재생합니다.");
    }
}